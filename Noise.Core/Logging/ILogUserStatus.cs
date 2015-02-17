using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogUserStatus {
		void	OpeningLibrary( LibraryConfiguration library );
		void	LibraryChanged( DatabaseChangeSummary summary );
		void	LibraryStatistics( DatabaseStatistics statistics );

		void	StartingLibraryUpdate();
		void	StartedLibraryDiscovery();
		void	StartedLibraryCleaning();
		void	StartedLibraryClassification();
		void	StartedLibrarySummary();
		void	StartedSidecarBuilding();
		void	CompletedLibraryUpdate();

		void	BuiltDecadeTags( DbArtist artist );
		void	BuiltSearchData( DbArtist artist );
		void	UpdatedAlbumPublishedYear( DbAlbum album );
		void	CalculatedReplayGain( DbArtist artist, DbAlbum album );
	}
}
