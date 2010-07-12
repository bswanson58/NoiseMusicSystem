using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public interface IMetaDataProvider {
		string	Artist { get; }
		string	Album {get; }

		void	AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track );
	}
}
