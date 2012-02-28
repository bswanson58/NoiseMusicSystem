using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TextInfoProvider : BaseProvider<DbTextInfo>, ITextInfoProvider {
		public TextInfoProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddTextInfo( DbTextInfo info, string filePath ) {
			throw new System.NotImplementedException();
		}

		public void AddTextInfo( DbTextInfo info ) {
			throw new System.NotImplementedException();
		}

		public void DeleteTextInfo( DbTextInfo textInfo ) {
			throw new System.NotImplementedException();
		}

		public TextInfo GetArtistTextInfo( long artistId, ContentType ofType ) {
			throw new System.NotImplementedException();
		}

		public TextInfo[] GetAlbumTextInfo( long albumId ) {
			throw new System.NotImplementedException();
		}

		public IDataUpdateShell<TextInfo> GetTextInfoForUpdate( long textInfoId ) {
			throw new System.NotImplementedException();
		}
	}
}
