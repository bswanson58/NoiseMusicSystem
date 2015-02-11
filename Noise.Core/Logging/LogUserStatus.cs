using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public class LogUserStatus : ILogUserStatus {
		private readonly IEventAggregator	mEventAggregator;

		public LogUserStatus( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		private void LogStatus( string statusMessage, bool extended = false ) {
			mEventAggregator.Publish( new Events.StatusEvent( statusMessage ) { ExtendDisplay = extended });
		}

		public void BuiltDecadeTags( DbArtist artist ) {
			LogStatus( string.Format( "Built decade tag associations for: {0}", artist.Name ));
		}

		public void BuiltSearchData( DbArtist artist ) {
			LogStatus( string.Format( "Built search data for: {0}", artist.Name ));
		}

		public void UpdatedAlbumPublishedYear( DbAlbum album ) {
			LogStatus( string.Format( "Updated published year from discography: album '{0}', year: '{1}'", album.Name, album.PublishedYear ));
		}

		public void CalculatedReplayGain( DbArtist artist, DbAlbum album ) {
			LogStatus( string.Format( "Calculated ReplayGain values for: {0}/{1}", artist.Name, album.Name ));
		}

		public void OpeningLibrary( LibraryConfiguration library ) {
			LogStatus( string.Format( "Opening library: {0}", library.LibraryName ));
		}

		public void StartingLibraryUpdate() {
			LogStatus( "Starting library update." );
		}

		public void StartedSidecarBuilding() {
			LogStatus( "Started sidecar update." );
		}

		public void CompletedLibraryUpdate() {
			LogStatus( "Completed library update." );
		}

		public void LibraryChanged( DatabaseChangeSummary summary ) {
			LogStatus( string.Format( "Library status: {0}", summary ), true );
		}

		public void LibraryStatistics( DatabaseStatistics statistics ) {
			LogStatus( statistics.ToString(), true );
		}

		public void StartedLibraryDiscovery() {
			LogStatus( "Started library discovery." );
		}

		public void StartedLibraryCleaning() {
			LogStatus( "Started library cleaning." );
		}

		public void StartedLibraryClassification() {
			LogStatus( "Started library classification." );
		}

		public void StartedLibrarySummary() {
			LogStatus( "Started building library summary." );
		}
	}
}
