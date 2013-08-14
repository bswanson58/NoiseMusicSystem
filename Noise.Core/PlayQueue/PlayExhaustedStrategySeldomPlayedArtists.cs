﻿using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayExhaustedStrategySeldomPlayedArtists : PlayExhaustedStrategyRandomBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly List<DbArtist>		mArtists; 

		public PlayExhaustedStrategySeldomPlayedArtists( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.SeldomPlayedArtists, albumProvider, trackProvider ) {
			mArtistProvider = artistProvider;

			mArtists = new List<DbArtist>();
		}

		protected override DbTrack SelectATrack() {
			var retValue = default( DbTrack );

			if(!mArtists.Any()) {
				LoadArtists();
			}

			if( mArtists.Any()) {
				var artist = mArtists.Skip( NextRandom( mArtists.Count - 1 )).Take( 1 ).FirstOrDefault();

				if( artist != null ) {
					mArtists.Remove( artist );

					retValue = RandomTrackFromArtist( artist );
				}
			}

			return( retValue );
		}

		private void LoadArtists() {
			using( var artistList = mArtistProvider.GetArtistList()) {
				if( artistList.List != null ) {
					mArtists.AddRange(( from artist in artistList.List where artist.Rating >= 0
										orderby artist.PlayCount ascending, artist.LastPlayedTicks ascending select artist ).Take( 10 ));
				}
			}
		}
	}
}
