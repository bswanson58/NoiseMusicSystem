using Caliburn.Micro;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.ArtistMetadata {
	public class ArtistMetadataManager : IArtistMetadataManager {
		private readonly IEventAggregator	mEventAggregator;

		public ArtistMetadataManager( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		public void Initialize() {
		}

		public void Shutdown() {
		}

		public void ArtistMentioned( string artistName ) {
		}

		public void ArtistForgotten( string artistName ) {
		}

		public void ArtistMetadataRequested( string artistName ) {
		}
	}
}
