using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.BackgroundTasks;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	[Export( typeof( IBackgroundTask ))]
	internal class AlbumSidecarSync : IBackgroundTask,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string				cSidecarSyncId		= "ComponentId_AlbumSidecar_Sync";

		private readonly IEventAggregator	mEventAggregator;
		private readonly INoiseLog			mLog;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ISidecarProvider	mSidecarProvider;
		private readonly SidecarCreator		mSidecarCreator;
		private readonly SidecarWriter		mSidecarWriter;
		private readonly List<long>			mAlbumList; 
		private IEnumerator<long>			mAlbumEnum; 

		public string TaskId { get; private set; }

		public AlbumSidecarSync( IEventAggregator eventAggregator, INoiseLog log, IAlbumProvider albumProvider,
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
						var albumSidecar = mSidecarCreator.CreateFromAlbum( album );
						var dbSideCar = mSidecarProvider.GetSidecarForAlbum( album ) ?? new StorageSidecar();

						if( albumSidecar.Version > dbSideCar.Version ) {
							mSidecarWriter.WriteSidecar( album, albumSidecar );

							mLog.LogMessage( string.Format( "Sidecar updated for {0}", album ));
						}

						if( dbSideCar.Status == SidecarStatus.Unread ) {
							mSidecarCreator.UpdateAlbum( album, mSidecarWriter.ReadSidecar( album ));

							using( var updater = mSidecarProvider.GetSidecarForUpdate( dbSideCar.DbId )) {
								if( updater.Item != null ) {
									updater.Item.Status = SidecarStatus.Read;

									updater.Update();

									mLog.LogMessage( string.Format( "Sidecar read for {0}", album ));
								}
							}
						}
						else {
							var storageSidecar = mSidecarWriter.ReadSidecar( album );

							if( storageSidecar.Version > dbSideCar.Version ) {
								using( var updater = mSidecarProvider.GetSidecarForUpdate( dbSideCar.DbId ))
								if( updater.Item != null ) {
									updater.Item.Version = storageSidecar.Version;

									updater.Update();
								}

								using( var updater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
									if( updater.Item != null ) {
										mSidecarCreator.UpdateAlbum( updater.Item, storageSidecar );

										updater.Update();
									}
								}

								mLog.LogMessage( string.Format( "Album updated from sidecar {0}", album ));
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
				}
				else {
					retValue = mAlbumProvider.GetAlbum( mAlbumEnum.Current );
				}
			}

			return( retValue );
		}

		private void InitializeLists() {
			try {
				mAlbumList.Clear();

				using( var artistList = mAlbumProvider.GetAllAlbums()) {
					mAlbumList.AddRange( from DbArtist artist in artistList.List select artist.DbId );
				}
				mAlbumEnum = mAlbumList.GetEnumerator();
			}
			catch( Exception ex ) {
				mLog.LogException( "Initialize album list", ex );
			}
		}
	}
}
