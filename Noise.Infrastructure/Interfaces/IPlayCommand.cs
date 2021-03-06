﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayCommand {
		Task	Play( DbArtist artist );
		Task	PlayRandomArtistTracks( DbArtist artist );
		Task	PlayTopArtistTracks( DbArtist artist );
        Task    PlayRandomTaggedTracks( DbTag tag );

		Task	Play( DbAlbum album );
		Task	Play( DbAlbum album, string volumeName );

		Task	Play( DbTrack track );
		Task	PlayNext( DbTrack track );
		Task	Play( IEnumerable<DbTrack> trackList );

		Task	Play( DbInternetStream stream );
	}
}
