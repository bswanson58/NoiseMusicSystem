using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Dto {
	public class LibraryExplorerFilter : IDatabaseFilter {
		public	bool	IsEnabled { get; set; }
		public	bool	OnlyFavorites { get; set; }

		public bool ArtistMatch( DbArtist artist ) {
			var retValue = false;

			if( IsEnabled ) {
				if( OnlyFavorites ) {
					if(( artist.IsFavorite ) ||
					   ( artist.HasFavorites )) {
						retValue = true;
					}
				}
				else {
					retValue = true;
				}
			}
			else {
				retValue = true;
			}

			return( retValue );
		}
	}
}
