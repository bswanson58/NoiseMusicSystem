using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ReplayGainTask : IBackgroundTask,
								  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>, 
								  IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILogBackgroundTasks	mLog;
		private readonly ILogUserStatus			mUserStatus;
		private readonly IReplayGainScanner		mReplayGainScanner;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly ISidecarUpdater		mSidecarUpdater;
		private readonly List<DbAlbum>			mAlbumList;
		private readonly bool					mReplayGainEnabled;
		private IEnumerator<DbAlbum>			mAlbumEnumerator;
		private bool							mScanEnabled;

		public ReplayGainTask( IEventAggregator eventAggregator, IReplayGainScanner scanner,
							   IPreferences preferences, ILogUserStatus userStatus, ILogBackgroundTasks log,
							   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							   IStorageFileProvider storageFileProvider, IStorageFolderSupport storageFolderSupport, ISidecarUpdater sidecarUpdater ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mUserStatus = userStatus;
			mReplayGainScanner = scanner;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mSidecarUpdater = sidecarUpdater;

			mAlbumList = new List<DbAlbum>();

			var audioCongfiguration = preferences.Load<AudioPreferences>();
			if( audioCongfiguration != null ) {
				mReplayGainEnabled = audioCongfiguration.ReplayGainEnabled;
			}

			mEventAggregator.Subscribe( this );
		}

		public string TaskId {
			get { return ( "Task_ReplayGainScanner" ); }
		}

		public void Handle( Events.DatabaseOpened message ) {
			InitializeScan();
		}

		public void Handle( Events.DatabaseClosing message ) {
			HaltScan();
		}

		public void Handle( Events.LibraryUpdateStarted args ) {
			HaltScan();
		}

		public void Handle( Events.LibraryUpdateCompleted args ) {
			InitializeScan();
		}

		private DbAlbum NextAlbum() {
			DbAlbum	retValue = null;

			if(( mAlbumEnumerator != null ) &&
			   ( mAlbumEnumerator.MoveNext())) {
				retValue = mAlbumEnumerator.Current;
			}

			return ( retValue );
		}

		private void InitializeScan() {
			if( mReplayGainEnabled ) {
				InitializeScanList();

				mScanEnabled = true;
			}
		}

		private void HaltScan() {
			mAlbumList.Clear();
			mAlbumEnumerator = null;

			mScanEnabled = false;
		}

		public void ExecuteTask() {
			if( mScanEnabled ) {
				var spinCount = 10;

				while( spinCount > 0 ) {
					var album = NextAlbum();

					spinCount--;

					if( album != null ) {
						try {
							var	trackList = new List<DbTrack>();
							var artist = mArtistProvider.GetArtistForAlbum( album );

							using( var tracks = mTrackProvider.GetTrackList( album )) {
								if( tracks.List != null ) {
									trackList.AddRange( tracks.List );
								}
							}

							if( trackList.Any()) {
								var scanRequired = trackList.Any( track => ( Math.Abs( track.ReplayGainTrackGain ) + Math.Abs( track.ReplayGainAlbumGain )) < 0.001f );

								if( scanRequired ) {
									if( ExecuteScanner( album, trackList )) {
										mUserStatus.CalculatedReplayGain( artist, album );

										mSidecarUpdater.UpdateSidecar( mAlbumProvider.GetAlbum( album.DbId ));

										mLog.ReplayGainScanCompleted( artist, album );
									}
									else {
										mLog.ReplayGainScanFailed( artist, album );
									}

									spinCount = 0;
								}
							}
						}
						catch( Exception ex ) {
							mLog.LogException( string.Format( "ReplayGainTask scanning: {0}", album ), ex );

							spinCount = 0;
						}
					}
				}
				
			}
		}

		private bool ExecuteScanner( DbAlbum album, IEnumerable<DbTrack> trackList ) {
			var retValue = false;

			mReplayGainScanner.ResetScanner();

			foreach( var track in trackList ) {
				var file = mStorageFileProvider.GetPhysicalFile( track );
				var path = mStorageFolderSupport.GetPath( file );

				mReplayGainScanner.AddFile( track.DbId, path );
			}

			if( mReplayGainScanner.CalculateReplayGain()) {
				if( mScanEnabled ) {
					var albumGain = mReplayGainScanner.AlbumGain;

					foreach( var file in mReplayGainScanner.FileList ) {
						using( var trackUpdater = mTrackProvider.GetTrackForUpdate( file.FileId )) {
							if( trackUpdater.Item != null ) {
								trackUpdater.Item.ReplayGainTrackGain = (float)file.TrackGain;
								trackUpdater.Item.ReplayGainAlbumGain = (float)albumGain;

								trackUpdater.Update();
							}
						}
					}

					using( var updater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
						if( updater.Item != null ) {
							updater.Item.ReplayGainAlbumGain = (float)albumGain;
							album.ReplayGainAlbumGain = (float)albumGain;

							updater.Update();
						}
					}

					retValue = true;
				}
			}

			return( retValue );
		}

		private void InitializeScanList() {
			mAlbumList.Clear();

			using( var albumList = mAlbumProvider.GetAllAlbums()) {
				if( albumList.List != null ) {
					mAlbumList.AddRange( albumList.List );
				}
			}

			mAlbumEnumerator = mAlbumList.GetEnumerator();
		}
	}
}
