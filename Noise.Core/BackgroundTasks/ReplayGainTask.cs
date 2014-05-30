using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ReplayGainTask : IBackgroundTask,
								  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IReplayGainScanner		mReplayGainScanner;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly List<DbAlbum>			mAlbumList;
		private readonly bool					mReplayGainEnabled;
		private IEnumerator<DbAlbum>			mAlbumEnumerator;
		private bool							mDatabaseOpen;

		public ReplayGainTask( IEventAggregator eventAggregator, IReplayGainScanner scanner, IPreferences preferences,
							   IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
							   IStorageFileProvider storageFileProvider, IStorageFolderSupport storageFolderSupport ) {
			mEventAggregator = eventAggregator;
			mReplayGainScanner = scanner;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
			mStorageFolderSupport = storageFolderSupport;

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
			if( mReplayGainEnabled ) {
				InitializeScanList();
			}

			mDatabaseOpen = true;
		}

		public void Handle( Events.DatabaseClosing message ) {
			mAlbumList.Clear();
			mAlbumEnumerator = null;

			mDatabaseOpen = false;
		}

		private DbAlbum NextAlbum() {
			DbAlbum	retValue = null;

			if( mAlbumEnumerator.MoveNext()) {
				retValue = mAlbumEnumerator.Current;
			}

			return ( retValue );
		}

		public void ExecuteTask() {
			if(( mReplayGainEnabled ) &&
			   ( mDatabaseOpen )) {
				var spinCount = 10;

				while( spinCount > 0 ) {
					var album = NextAlbum();

					spinCount--;

					if( album != null ) {
						var albumName = album.Name;

						try {
							var	trackList = new List<DbTrack>();
							var artist = mArtistProvider.GetArtistForAlbum( album );

							if( artist != null ) {
								albumName = string.Format( "{0}/{1}", artist.Name, album.Name );
							}

							using( var tracks = mTrackProvider.GetTrackList( album )) {
								if( tracks.List != null ) {
									trackList.AddRange( tracks.List );
								}
							}

							if( trackList.Any()) {
								var scanRequired = trackList.Any( track => ( Math.Abs( track.ReplayGainTrackGain ) + Math.Abs( track.ReplayGainAlbumGain )) < 0.001f );

								if( scanRequired ) {
									mEventAggregator.Publish( new Events.StatusEvent( string.Format( "Calculating ReplayGain values for: {0}", albumName )));

									if( ExecuteScanner( trackList )) {
										NoiseLogger.Current.LogMessage( "ReplayGainScanner updated album: '{0}' - Album gain: {1:N2}", albumName, mReplayGainScanner.AlbumGain );
									}
									else {
										NoiseLogger.Current.LogMessage( "ReplayGainScanned failed for album: {0}", albumName );
									}

									spinCount = 0;
								}
							}
						}
						catch( Exception ex ) {
							NoiseLogger.Current.LogException( string.Format( "ReplayGainTask scanning album '{0}'", albumName ), ex );

							spinCount = 0;
						}
					}
				}
				
			}
		}

		private bool ExecuteScanner( IEnumerable<DbTrack> trackList ) {
			var retValue = false;

			mReplayGainScanner.ResetScanner();

			foreach( var track in trackList ) {
				var file = mStorageFileProvider.GetPhysicalFile( track );
				var path = mStorageFolderSupport.GetPath( file );

				mReplayGainScanner.AddFile( track.DbId, path );
			}

			if( mReplayGainScanner.CalculateReplayGain()) {
				if( mDatabaseOpen ) {
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

					retValue = true;
				}
			}

			return( retValue );
		}

		private void InitializeScanList() {
			using( var albumList = mAlbumProvider.GetAllAlbums()) {
				if( albumList.List != null ) {
					mAlbumList.AddRange( albumList.List );
				}
			}

			mAlbumEnumerator = mAlbumList.GetEnumerator();
		}
	}
}
