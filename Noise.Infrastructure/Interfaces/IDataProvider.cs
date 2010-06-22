using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure {
	public interface IDataProvider {
		IEnumerable<DbArtist>	GetArtistList();
		IEnumerable<DbAlbum>	GetAlbumList( DbArtist forArtist );
	}
}
