using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IRandomTrackSelector {
		IEnumerable<DbTrack>	SelectTracks( DbArtist fromArtist, Func<DbTrack, bool> approveTrack, int count ); 
		IEnumerable<DbTrack>	SelectTracks( IEnumerable<DbAlbum> albumList, Func<DbTrack, bool> approveTrack, int count ); 
	}
}
