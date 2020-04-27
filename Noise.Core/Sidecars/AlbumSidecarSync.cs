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
	internal class AlbumSidecarSync : IBackgroundTask,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string				cSidecarSyncId		= "ComponentId_AlbumSidecar_Sync";

		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly ISidecarCreator				mSidecarCreator;
		private readonly ISidecarWriter					mSidecarWriter;
		private readonly List<long>						mAlbumList; 
		private IEnumerator<long>						mAlbumEnum; 

		public string TaskId { get; }

		public AlbumSidecarSync( IEventAggregator eventAggregator, ILogLibraryBuildingSidecars log, IAlbumProvider albumProvider,
								 ISidecarProvider sidecarProvider, ISidecarCreator sidecarCreator, ISidecarWriter sidecarWriter ) {
			TaskId = cSidecarSyncId;

			mLog = log;
			mAlbumProvider = albumProvider;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mSidecarWriter = sidecarWriter;

			mAlbumList = new List<long>();

			eventAggregator.Subscribe( this );
		}

        public void ExecuteTask() {
            // process a block of sidecars in each time slot.
            for( var task = 0; task < 10; task++ ) {
                CheckNextAlbum();
            }
        }

		private void CheckNextAlbum() {
			var album = NextAlbum();

			if( album != null ) {
				try {
					if( mSidecarWriter.IsStorageAvailable( album )) {
						var dbSideCar = mSidecarProvider.GetSidecarForAlbum( album );
						var albumSidecar = mSidecarCreator.CreateFrom( album );

						 if(( dbSideCar != null ) &&
						   ( albumSidecar != null ) &&
                           ( albumSidecar.Version != dbSideCar.Version )) {
							if( albumSidecar.Version > dbSideCar.Version ) {
								var existingSidecar = mSidecarWriter.ReadSidecar( album );

								mSidecarCreator.UpdateSidecar( existingSidecar, album );
                                mSidecarWriter.WriteSidecar( album, existingSidecar );
                                mSidecarWriter.UpdateSidecarVersion( album, dbSideCar );
							}
							else {
								var storageSidecar = mSidecarWriter.ReadSidecar( album );

								if(( storageSidecar != null ) &&
								   ( storageSidecar.Version > dbSideCar.Version )) {
									mSidecarCreator.Update( album, storageSidecar );

									// Get the updated album version.
									album = mAlbumProvider.GetAlbum( album.DbId );
									if( album != null ) {
										mSidecarWriter.UpdateSidecarVersion( album, dbSideCar );
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
					mLog.LogException( $"Syncing sidecar for {album}", exception );
				}
			}
		}

		public void Handle( Events.DatabaseOpened message ) {
			InitializeLists();
		}

		public void Handle( Events.DatabaseClosing message ) {
			mAlbumList.Clear();
			mAlbumEnum = null;
		}

		private DbAlbum NextAlbum() {
			var	retValue = default( DbAlbum );

			if( mAlbumEnum != null ) {
				if(!mAlbumEnum.MoveNext()) {
					InitializeLists();

					mAlbumEnum.MoveNext();
				}

				retValue = mAlbumProvider.GetAlbum( mAlbumEnum.Current );
			}

			return( retValue );
		}

		private void InitializeLists() {
			try {
				mAlbumList.Clear();

				using( var albumList = mAlbumProvider.GetAllAlbums()) {
					mAlbumList.AddRange( from DbAlbum album in albumList.List select album.DbId );
				}
				mAlbumEnum = mAlbumList.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "Initializing album list", ex );
			}
		}
	}
}
