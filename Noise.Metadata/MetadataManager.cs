using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,IHandle<Events.ArtistContentRequest> {
		private readonly IEventAggregator	mEventAggregator;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );

			mEventAggregator.Subscribe( this );
		}

		public void Initialize() {
		}

		public void Shutdown() {
		}

		public void Handle( Events.ArtistAdded args ) {
			
		}

		public void Handle( Events.ArtistRemoved args ) {
			
		}

		public void Handle( Events.ArtistContentRequest args ) {
			
		}
	}
}
