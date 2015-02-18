using System;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogBackgroundTasks {
		void	LogTasksStarting( int count );
		void	LogTasksStopping();

		void	StartingDecadeTagBuilding( DbArtist artist );
		void	CompletedDecadeTagBuilding( DbArtist artist );

		void	StartingDiscographyExploring( DbArtist artist );
		void	UpdatedFromDiscography( DbAlbum album );
		void	CompletedDiscographyExploring( DbArtist artist );

		void	ReplayGainScanCompleted( DbArtist artist, DbAlbum album );
		void	ReplayGainScanFailed( DbArtist artist, DbAlbum album );

		void	StartingSearchBuilding( DbArtist artist );
		void	CompletedSearchBuilding( DbArtist artist );

		void	LogException( string message, Exception exception, string callerName = "" );
	}
}
