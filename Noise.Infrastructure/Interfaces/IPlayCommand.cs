using System.Collections.Generic;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayCommand {
		Task	Play( DbArtist artist );
		Task	Play( DbAlbum album );
		Task	Play( DbAlbum album, string volumeName );
		Task	Play( DbTrack track );
		Task	Play( IEnumerable<DbTrack> trackList );
		Task	Play( DbInternetStream stream );
	}
}
