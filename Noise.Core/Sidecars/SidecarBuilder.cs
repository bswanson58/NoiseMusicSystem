using System;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarBuilder : ISidecarBuilder {
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly ILogUserStatus					mUserLog;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISidecarProvider				mSidecarProvider;
		private readonly SidecarCreator					mSidecarCreator;
		private readonly SidecarWriter					mSidecarWriter;
		private bool									mStopBuilding;

		public SidecarBuilder( ISidecarProvider sidecarProvider, IArtistProvider artistProvider, IAlbumProvider albumProvider,
							   SidecarCreator sidecarCreator, SidecarWriter sidecarWriter,
							   ILogLibraryBuildingSidecars log, LogUserStatus userLog ) {
			mLog = log;
			mUserLog = userLog;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mSidecarProvider = sidecarProvider;
			mSidecarCreator = sidecarCreator;
			mSidecarWriter = sidecarWriter;
		}

		public void Process() {
			mStopBuilding = false;
			mLog.LogSidecarBuildingStarted();
			mUserLog.StartedSidecarBuilding();

			try {
				using( var unreadList = mSidecarProvider.GetUnreadSidecars()) {
					foreach( var sidecar in unreadList.List ) {
						if( sidecar.IsAlbumSidecar ) {
							ProcessAlbumSidecar( sidecar );
						}

						if( sidecar.IsArtistSidecar ) {
							ProcessArtistSidecar( sidecar );
						}

						if( mStopBuilding ) {
							break;
						}
					}
				}

				using( var albumList = mAlbumProvider.GetAllAlbums()) {
					foreach( var album in albumList.List ) {
						if( mSidecarProvider.GetSidecarForAlbum( album ) == null ) {
							mSidecarProvider.Add( new StorageSidecar( Constants.AlbumSidecarName, album ) { Status = SidecarStatus.Read });
						}
					}
				}

				using( var artistList = mArtistProvider.GetArtistList()) {
					foreach( var artist in artistList.List ) {
						if( mSidecarProvider.GetSidecarForArtist( artist ) == null ) {
							mSidecarProvider.Add( new StorageSidecar( Constants.ArtistSidecarName, artist ) { Status = SidecarStatus.Read });
						}
					}
				}
			}
			catch( Exception exception ) {
				mLog.LogException( "Building sidecars", exception );
			}

			mLog.LogSidecarBuildingCompleted();
		}

		private void ProcessAlbumSidecar( StorageSidecar sidecar ) {
			var album = mAlbumProvider.GetAlbum( sidecar.AlbumId );

			if( album != null ) {
				mSidecarCreator.Update( album, mSidecarWriter.ReadSidecar( album ));
				UpdateSidecarVersion( sidecar, album.Version );
			}
			else {
				mLog.LogUnknownAlbumSidecar( sidecar );
			}
		}

		private void ProcessArtistSidecar( StorageSidecar sidecar ) {
			var artist = mArtistProvider.GetArtist( sidecar.ArtistId );

			if( artist != null ) {
				mSidecarCreator.Update( artist, mSidecarWriter.ReadSidecar( artist ));
				UpdateSidecarVersion( sidecar, artist.Version );
			}
			else {
				mLog.LogUnknownArtistSidecar( sidecar );
			}
		}

		private void UpdateSidecarVersion( StorageSidecar sidecar, long version ) {
			using( var updater = mSidecarProvider.GetSidecarForUpdate( sidecar.DbId )) {
				if( updater.Item != null ) {
					updater.Item.Status = SidecarStatus.Read;
					updater.Item.Version = version;

					updater.Update();
				}
			}
		}

		public void Stop() {
			mStopBuilding = true;
		}
	}
}
