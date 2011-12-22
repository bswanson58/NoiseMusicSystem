using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITextInfoProvider {
		TextInfo	GetArtistTextInfo( long artistId, ContentType ofType );
		TextInfo	GetAlbumTextInfo( long albumId );
	}
}
