using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITextInfoProvider {
		void						AddTextInfo( DbTextInfo info, string filePath );
		void						AddTextInfo( DbTextInfo info );
		void						DeleteTextInfo( DbTextInfo textInfo );

		TextInfo					GetArtistTextInfo( long artistId, ContentType ofType );
		TextInfo[]					GetAlbumTextInfo( long albumId );

		DataUpdateShell<TextInfo>	GetTextInfoForUpdate( long textInfoId );
	}
}
