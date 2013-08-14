using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	public class PlayExhaustedStrategyArtistGenre : PlayExhaustedStrategyRandomBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly List<DbArtist>		mArtistList;
		private long						mGenre;

		public PlayExhaustedStrategyArtistGenre( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( ePlayExhaustedStrategy.PlayArtistGenre, albumProvider, trackProvider ) {
			mArtistProvider = artistProvider;

			mArtistList = new List<DbArtist>();
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( parameters is PlayStrategyParameterDbId ) {
				var parms = parameters as PlayStrategyParameterDbId;

				mGenre = parms.DbItemId;
			}
		}

		protected override DbTrack SelectATrack() {
			var retValue = default( DbTrack );

			if(!mArtistList.Any()) {
				LoadArtists( mGenre );
			}

			if( mArtistList.Any()) {
				retValue = RandomTrackFromArtist( mArtistList.Skip( NextRandom( mArtistList.Count -1 )).Take( 1 ).FirstOrDefault());				
			}

			return( retValue );
		}

		private void LoadArtists( long genreId ) {
			using( var artists = mArtistProvider.GetArtistList() ) {
				if( artists.List != null ) {
					mArtistList.AddRange( from artist in artists.List where artist.Genre == genreId select artist );
				}
			}
		}
	}
}
