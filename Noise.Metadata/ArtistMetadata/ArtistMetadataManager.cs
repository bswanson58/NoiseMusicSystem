using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;

namespace Noise.Metadata.ArtistMetadata {
	public class ArtistMetadataManager : IArtistMetadataManager {
		private IDocumentStore				mDocumentStore;

		public void Initialize( IDocumentStore documentStore ) {
			mDocumentStore = documentStore;
		}

		public void Shutdown() {
			mDocumentStore = null;
		}

		public void ArtistMentioned( string artistName ) {
			InsureArtistStatus( artistName );
		}

		public void ArtistForgotten( string artistName ) {
		}

		public IArtistMetadata GetArtistBiography( string forArtist ) {
			return( GetOrCreateArtistBiography( forArtist ));
		}

		private void InsureArtistStatus( string forArtist ) {
			if( mDocumentStore != null ) {
				using( var session = mDocumentStore.OpenSession()) {
					var	status = session.Load<DbArtistStatus>( DbArtistStatus.FormatStatusKey( forArtist ));

					if( status == null ) {
						status = new DbArtistStatus { ArtistName = forArtist };

						session.Store( status );
						session.SaveChanges();
					}
				}
			}
		}

		private DbArtistBiography GetOrCreateArtistBiography( string forArtist ) {
			var retValue = default( DbArtistBiography );

			if( mDocumentStore != null ) {
				using( var session = mDocumentStore.OpenSession()) {
					retValue = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( forArtist ));
				}
			}

			if( retValue == null ) {
				retValue = new DbArtistBiography { ArtistName = forArtist };
			}

			return( retValue );
		}
	}
}
