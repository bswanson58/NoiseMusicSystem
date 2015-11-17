using System;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Sidecars {
	internal class SidecarCreator : ISidecarCreator {
		private readonly ILogLibraryBuildingSidecars	mLog;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;

		public SidecarCreator( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ILogLibraryBuildingSidecars log ) {
			mLog = log;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public ScArtist CreateFrom( DbArtist artist ) {
			return( new ScArtist( artist ));
		}

		public ScAlbum CreateFrom( DbAlbum album ) {
			var retValue = new ScAlbum( album );

			using( var trackList = mTrackProvider.GetTrackList( album ) ) {
				foreach( var track in trackList.List ) {
					retValue.TrackList.Add( new ScTrack( track ));
				}
			}

			return( retValue );
		}

		public void Update( DbArtist artist, ScArtist sidecar ) {
			using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
				if( updater.Item != null ) {
					sidecar.UpdateArtist( updater.Item );
					updater.Item.SetVersionPreUpdate( sidecar.Version );

					updater.Update();

					sidecar.UpdateArtist( artist );
					mLog.LogUpdated( artist, sidecar );
				}
			}
		}

		public void Update( DbAlbum album, ScAlbum sidecar ) {
			using( var updater = mAlbumProvider.GetAlbumForUpdate( album.DbId )) {
				if( updater.Item != null ) {
					sidecar.UpdateAlbum( updater.Item );
					updater.Item.SetVersionPreUpdate( sidecar.Version );

					updater.Update();

					sidecar.UpdateAlbum( album );
					mLog.LogUpdated( album, sidecar );
				}
			}

			using( var trackList = mTrackProvider.GetTrackList( album )) {
				foreach( var scTrack in sidecar.TrackList ) {
					var dbTrack = trackList.List.FirstOrDefault( track => track.Name.Equals( scTrack.TrackName, StringComparison.CurrentCultureIgnoreCase ) &&
																		  track.TrackNumber == scTrack.TrackNumber &&
																		  track.VolumeName.Equals( scTrack.VolumeName, StringComparison.CurrentCultureIgnoreCase ));
					if( dbTrack != null ) {
						using( var updater = mTrackProvider.GetTrackForUpdate( dbTrack.DbId )) {
							if( updater.Item != null ) {
								scTrack.UpdateTrack( updater.Item );

								updater.Update();
							}
						}
					}
					else {
						mLog.LogUnknownTrack( album, scTrack );
					}
				}
			}
		}
	}
}
