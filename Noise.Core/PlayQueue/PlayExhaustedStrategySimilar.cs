﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
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
		public PlayExhaustedStrategySimilar( IUnityContainer container ) :
			base( container ) { }

		protected override void FillTrackList() {
			mTrackList.Clear();

			try {
				var manager = mContainer.Resolve<INoiseManager>();
				var artistList = mQueueMgr.PlayList.Select( item => item.Artist ).Distinct( new ArtistComparer());

				foreach( var artist in artistList ) {
					var supportInfo = manager.DataProvider.GetArtistSupportInfo( artist );

					foreach( var item in supportInfo.SimilarArtist.Items ) {
						if( item.IsLinked ) {
							var associatedArtist = manager.DataProvider.GetArtist( item.AssociatedId );

							if( associatedArtist != null ) {
								using( var albumList = manager.DataProvider.GetAlbumList( associatedArtist ) ) {
									foreach( var album in albumList.List ) {
										using( var trackList = manager.DataProvider.GetTrackList( album ) ) {
											mTrackList.AddRange( trackList.List );
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
				mLog.LogException( "Exception - PlayExhaustedStrategySimilar: ", ex );
			}
		}
	}
}
