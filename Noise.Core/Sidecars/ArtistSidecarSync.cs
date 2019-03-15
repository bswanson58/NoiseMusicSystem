using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.BackgroundTasks;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	[Export( typeof( IBackgroundTask ))]
	internal class ArtistSidecarSync : IBackgroundTask,
									   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string				cSidecarSyncId		= "ComponentId_ArtistSidecar_Sync";

		private readonly IEventAggregator				mEventAggregator;
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IArtistProvider				mArtistProvider;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly ISidecarCreator				mSidecarCreator;
		private readonly ISidecarWriter					mSidecarWriter;
		private readonly List<long>						mArtistList; 
		private IEnumerator<long>						mArtistEnum; 

		public string TaskId { get; private set; }

		public ArtistSidecarSync( IEventAggregator eventAggregator, ILogLibraryBuildingSidecars log, IArtistProvider artistProvider,
								  ISidecarProvider sidecarProvider, ISidecarCreator sidecarCreator, ISidecarWriter sidecarWriter ) {
			TaskId = cSidecarSyncId;

			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mSidecarWriter = sidecarWriter;

			mArtistList = new List<long>();

			mEventAggregator.Subscribe( this );
		}

		public void ExecuteTask() {
			var artist = NextArtist();

			if( artist != null ) {
				try {
					if( mSidecarWriter.IsStorageAvailable( artist )) {
						var dbSideCar = mSidecarProvider.GetSidecarForArtist( artist );
						var artistSidecar = mSidecarCreator.CreateFrom( artist );

						if(( dbSideCar != null ) &&
						   ( artistSidecar != null )) {
							if( artistSidecar.Version > dbSideCar.Version ) {
								mSidecarWriter.WriteSidecar( artist, artistSidecar );
								mSidecarWriter.UpdateSidecarVersion( artist, dbSideCar );
							}
							else {
								var storageSidecar = mSidecarWriter.ReadSidecar( artist );

								if(( storageSidecar != null ) &&
								   ( storageSidecar.Version > dbSideCar.Version )) {
									mSidecarCreator.Update( artist, storageSidecar );

									// Get the updated artist version.
									artist = mArtistProvider.GetArtist( artist.DbId );
									if( artist != null ) {
										mSidecarWriter.UpdateSidecarVersion( artist, dbSideCar );
									}
									else {
										mLog.LogUnknownAlbumSidecar( dbSideCar );
									}
								}
							}
						}
					}
				}
				catch( Exception exception ) {
					mLog.LogException( string.Format( "Syncing sidecar for {0}", artist ), exception );
				}
			}
		}

		public void Handle( Events.DatabaseOpened message ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing message ) {
			mArtistList.Clear();
			mArtistEnum = null;
		}

		private DbArtist NextArtist() {
			var	retValue = default( DbArtist );

			if( mArtistEnum != null ) {
				if(!mArtistEnum.MoveNext()) {
					InitializeLists();

					mArtistEnum.MoveNext();
				}

				retValue = mArtistProvider.GetArtist( mArtistEnum.Current );
			}

			return( retValue );
		}

		private void InitializeLists() {
			try {
				mArtistList.Clear();

				using( var artistList = mArtistProvider.GetArtistList()) {
					mArtistList.AddRange( from DbArtist artist in artistList.List select artist.DbId );
				}
				mArtistEnum = mArtistList.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "Initializing artist list", ex );
			}
		}
	}
}
