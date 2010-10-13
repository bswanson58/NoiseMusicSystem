﻿using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayListMgr {
		List<DbPlayList>		PlayLists { get; }

		DbPlayList				Create( IEnumerable<PlayQueueTrack> fromList, string name, string description );
		IEnumerable<DbTrack>	GetTracks( DbPlayList forList );
	}
}
