using System.IO;
using Caliburn.Micro;
using Noise.BlobStorage.BlobStore;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.ArtistMetadata {
	public class ArtistMetadataManager : IArtistMetadataManager {
		private readonly IEventAggregator	mEventAggregator;
		private IBlobStorageManager			mBlobStorageManager;

		public ArtistMetadataManager( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		public void Initialize( IBlobStorageManager blobStorageManager ) {
			mBlobStorageManager = blobStorageManager;
		}

		public void Shutdown() {
		}

		public void ArtistMentioned( string artistName ) {
			RetrieveMetadataStatus( artistName );
		}

		public void ArtistForgotten( string artistName ) {
		}

		public void ArtistMetadataRequested( string artistName ) {
			RetrieveMetadataStatus( artistName );
		}

		private ArtistMetadataStatus RetrieveMetadataStatus( string forArtist ) {
			var retValue = default( ArtistMetadataStatus );

			if( mBlobStorageManager.IsOpen ) {
				var blobStorage = mBlobStorageManager.GetStorage();
				var statusPath = Path.Combine( forArtist, ArtistMetadataStatus.Filename );

				if( blobStorage.BlobExists( statusPath )) {
					retValue = blobStorage.RetrieveObject<ArtistMetadataStatus>( statusPath );
				}
				else {
					retValue = new ArtistMetadataStatus();

					blobStorage.Insert( statusPath, retValue );
				}
			}

			return( retValue );
		}
	}
}
