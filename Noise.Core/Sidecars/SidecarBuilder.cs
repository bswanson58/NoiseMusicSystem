using System;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarBuilder : ISidecarBuilder {
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly SidecarCreator					mSidecarCreator;
		private readonly SidecarWriter					mSidecarWriter;
		private bool									mStopBuilding;

		public SidecarBuilder( ISidecarProvider sidecarProvider, IAlbumProvider albumProvider, SidecarCreator sidecarCreator, SidecarWriter sidecarWriter,
							   ILogLibraryBuildingSidecars log ) {
			mLog = log;
			mAlbumProvider = albumProvider;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mSidecarWriter = sidecarWriter;
		}

		public void Process() {
			mStopBuilding = false;
			mLog.LogSidecarBuildingStarted();

			try {
				using( var unreadList = mSidecarProvider.GetUnreadSidecars()) {
					foreach( var sidecar in unreadList.List ) {
						var album = mAlbumProvider.GetAlbum( sidecar.AlbumId );

						if( album != null ) {
							mSidecarCreator.UpdateAlbum( album, mSidecarWriter.ReadSidecar( album ));

							using( var updater = mSidecarProvider.GetSidecarForUpdate( sidecar.DbId )) {
								if( updater.Item != null ) {
									updater.Item.Status = SidecarStatus.Read;
									updater.Item.Version = album.Version;

									updater.Update();
								}
							}
						}
						else {
							mLog.LogUnknownAlbumSidecar( sidecar );
						}

						if( mStopBuilding ) {
							break;
						}
					}
				}
			}
			catch( Exception exception ) {
				mLog.LogException( "Building sidecars", exception );
			}

			mLog.LogSidecarBuildingCompleted();
		}

		public void Stop() {
			mStopBuilding = true;
		}
	}
}
