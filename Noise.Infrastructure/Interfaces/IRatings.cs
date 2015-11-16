using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IRatings {
		void	SetRating( DbArtist artist, short rating );
		void	SetRating( DbAlbum album, short rating );
		void	SetRating( DbTrack track, short rating );

		void	SetFavorite( DbArtist artist, bool isFavorite );
		void	SetFavorite( DbAlbum album, bool isFavorite );
		void	SetFavorite( DbTrack track, bool isFavorite );
	}
}
