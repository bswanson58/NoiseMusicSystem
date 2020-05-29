using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public interface IMetadataInfoProvider {
		string	Artist { get; }
		string	Album {get; }
		string	TrackName { get; }
		string	VolumeName { get; }

		void	AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track );
	}
}
