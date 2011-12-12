using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
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
		private readonly IDataProvider	mDataProvider;

		public PlayExhaustedStrategySimilar( IDataProvider dataProvider ) {
			mDataProvider = dataProvider;
		}

		protected override void FillTrackList( long itemId ) {
			mTrackList.Clear();

			try {
				var artistList = mQueueMgr.PlayList.Select( item => item.Artist ).Distinct( new ArtistComparer());

				foreach( var artist in artistList ) {
					var supportInfo = mDataProvider.GetArtistSupportInfo( artist.DbId );

					foreach( var item in supportInfo.SimilarArtist.Items ) {
						if( item.IsLinked ) {
							var associatedArtist = mDataProvider.GetArtist( item.AssociatedId );

							if( associatedArtist != null ) {
								using( var albumList = mDataProvider.GetAlbumList( associatedArtist )) {
									foreach( var album in albumList.List ) {
										using( var trackList = mDataProvider.GetTrackList( album )) {
											foreach( var track in trackList.List ) {
												if(!mQueueMgr.IsTrackQueued( track )) {
													mTrackList.Add( track );
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
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - PlayExhaustedStrategySimilar: ", ex );
			}
		}
	}
}
