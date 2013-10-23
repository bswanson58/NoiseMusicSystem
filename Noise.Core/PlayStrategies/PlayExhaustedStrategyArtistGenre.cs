using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayExhaustedStrategyArtistGenre : PlayExhaustedStrategyRandomBase {
		private readonly IArtistProvider	mArtistProvider;
		private readonly IGenreProvider		mGenreProvider;
		private readonly List<DbArtist>		mArtistList;
		private long						mGenre;
		private string						mGenreName;

		public PlayExhaustedStrategyArtistGenre( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IGenreProvider genreProvider ) :
			base( ePlayExhaustedStrategy.PlayArtistGenre, "Play Genre...", true, albumProvider, trackProvider ) {
			mArtistProvider = artistProvider;
			mGenreProvider = genreProvider;

			mArtistList = new List<DbArtist>();
		}

		protected override string FormatDescription() {
			return( string.Format( "play tracks from genre {0}", mGenreName ));
		}

		protected override void ProcessParameters( IPlayStrategyParameters parameters ) {
			if( parameters is PlayStrategyParameterDbId ) {
				var parms = parameters as PlayStrategyParameterDbId;

				mGenre = parms.DbItemId;

				var genre = ( from g in mGenreProvider.GetGenreList().List where g.DbId == mGenre select g ).FirstOrDefault();
				if( genre != null ) {
					mGenreName = genre.Name;
				}
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
					mArtistList.AddRange( from artist in artists.List where artist.Genre == genreId && artist.Rating >= 0 select artist );
				}
			}
		}
	}
}
