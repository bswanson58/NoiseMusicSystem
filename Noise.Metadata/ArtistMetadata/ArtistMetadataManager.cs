using System.Linq;
using Caliburn.Micro;
using Noise.Metadata.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace Noise.Metadata.ArtistMetadata {
	public class ArtistMetadataManager : IArtistMetadataManager {
		private readonly IEventAggregator	mEventAggregator;
		private IDocumentStore				mDocumentStore;

		public ArtistMetadataManager( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		public void Initialize( IDocumentStore documentStore ) {
			mDocumentStore = documentStore;
		}

		public void Shutdown() {
			mDocumentStore = null;
		}

		public void ArtistMentioned( string artistName ) {
			GetOrCreateArtistMetadata( artistName );
		}

		public void ArtistForgotten( string artistName ) {
		}

		public void ArtistMetadataRequested( string artistName ) {
			var metadata = GetOrCreateArtistMetadata( artistName );
		}

		private ArtistMetadataInfo GetOrCreateArtistMetadata( string forArtist ) {
			ArtistMetadataInfo	retValue = null;

			if( mDocumentStore != null ) {
				using( var session = mDocumentStore.OpenSession()) {
					retValue = ( from metadata in session.Query<ArtistMetadataInfo>()
								 where metadata.ArtistName == forArtist 
								 select metadata ).FirstOrDefault();

					if( retValue == null ) {
						retValue = new ArtistMetadataInfo { ArtistName = forArtist };

						session.Store( retValue );
						session.SaveChanges();
					}
				}
			}

			return( retValue );
		}
	}
}
