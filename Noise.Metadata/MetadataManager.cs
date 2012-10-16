using Caliburn.Micro;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Support;

namespace Noise.Metadata {
	public class MetadataManager : IRequireInitialization,
								   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,IHandle<Events.ArtistContentRequest> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IBlobStorageManager	mBlobStorageManager;

		public MetadataManager( ILifecycleManager lifecycleManager,  IEventAggregator eventAggregator,
								IBlobStorageManager blobStorageManager, IMetadataStorageResolver storageResolver ) {
			mEventAggregator = eventAggregator;
			mBlobStorageManager = blobStorageManager;
			mBlobStorageManager.SetResolver( storageResolver );

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
