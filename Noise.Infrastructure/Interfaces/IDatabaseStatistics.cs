using System;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseStatistics {
		long		ArtistCount { get; }
		long		AlbumCount { get; }
		long		TrackCount { get; }

		long		FolderCount { get; }
		long		FileCount { get; }
		
		DateTime	LastScan { get; }
	}
}
