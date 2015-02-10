﻿using System;
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

		private readonly IEventAggregator				mEventAggregator;
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly SidecarCreator					mSidecarCreator;
		private readonly SidecarWriter					mSidecarWriter;
		private readonly List<long>						mAlbumList; 
		private IEnumerator<long>						mAlbumEnum; 

		public string TaskId { get; private set; }

		public AlbumSidecarSync( IEventAggregator eventAggregator, ILogLibraryBuildingSidecars log, IAlbumProvider albumProvider,
								 ISidecarProvider sidecarProvider, SidecarCreator sidecarCreator, SidecarWriter sidecarWriter ) {
			TaskId = cSidecarSyncId;

			mEventAggregator = eventAggregator;
			mLog = log;
			mAlbumProvider = albumProvider;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mSidecarWriter = sidecarWriter;

			mAlbumList = new List<long>();

			mEventAggregator.Subscribe( this );
		}

		public void ExecuteTask() {
			var album = NextAlbum();

			if( album != null ) {
				try {
					if( mSidecarWriter.IsStorageAvailable( album )) {
						var dbSideCar = mSidecarProvider.GetSidecarForAlbum( album );
						var albumSidecar = mSidecarCreator.CreateFromAlbum( album );

						if(( dbSideCar != null ) &&
						   ( albumSidecar != null )) {
							if( albumSidecar.Version > dbSideCar.Version ) {
								mSidecarWriter.WriteSidecar( album, albumSidecar );
								mSidecarWriter.UpdateSidecarVersion( album, dbSideCar );

								mLog.LogUpdatedSidecar( dbSideCar, album );
							}
							else {
								var storageSidecar = mSidecarWriter.ReadSidecar( album );

								if(( storageSidecar != null ) &&
									( storageSidecar.Version > dbSideCar.Version )) {
									mSidecarCreator.UpdateAlbum( album, storageSidecar );

									// Get the updated album version.
									album = mAlbumProvider.GetAlbum( album.DbId );
									if( album != null ) {
										mSidecarWriter.UpdateSidecarVersion( album, dbSideCar );

										mLog.LogUpdatedAlbum( dbSideCar, album );
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
					mLog.LogException( string.Format( "Syncing album sidecar for {0}", album ), exception );
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
				mLog.LogException( "Initialize album list", ex );
			}
		}
	}
}
