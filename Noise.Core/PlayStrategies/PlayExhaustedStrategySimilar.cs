using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class ArtistComparer : IEqualityComparer<DbArtist> {
		public bool Equals( DbArtist x, DbArtist y ) {
			if( x == null || y == null ) {
				return false;
			}

			return x.DbId == y.DbId;
		}

		public int GetHashCode( DbArtist obj ) {
			return obj.DbId.GetHashCode();
		}
	}

	internal class PlayExhaustedStrategySimilar : PlayExhaustedListBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly IMetadataManager	mMetadataManager;

		public PlayExhaustedStrategySimilar( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider,
											 IMetadataManager metadataManager, ILogPlayStrategy log ) :
			base( ePlayExhaustedStrategy.PlaySimilar, "Play Similar", "Play tracks from artists that are similar to the artists of tracks in the queue.", log ) {
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mMetadataManager = metadataManager;
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from similar artists" ));
		}

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				var artistList = PlayQueueMgr.PlayList.Select( item => item.Artist ).Distinct( new ArtistComparer());

				foreach( var artist in artistList ) {
					var artistBio = mMetadataManager.GetArtistMetadata( artist.Name );
					var similarArtsts = artistBio.GetMetadataArray( eMetadataType.SimilarArtists );

					foreach( var item in similarArtsts ) {
						var associatedArtist = mArtistProvider.FindArtist( item );

						if( associatedArtist != null ) {
							using( var albumList = mAlbumProvider.GetAlbumList( associatedArtist )) {
								foreach( var album in albumList.List ) {
									using( var trackList = mTrackProvider.GetTrackList( album )) {
										foreach( var track in trackList.List ) {
											if(( track.Rating >= 0 ) &&
											   (!PlayQueueMgr.IsTrackQueued( track ))) {
												mTrackList.Add( track );

												LogTrackAdded( track );
											}
										}
									}

									if( mTrackList.Count > 250 ) {
										break;
									}
								}
							}
						}
					}
				}
			}
			catch( Exception ex ) {
				Log.LogException( "Selecting a similar track", ex );
			}
		}
	}
}
