﻿using Caliburn.Micro;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;

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
			GetOrCreateArtistStatus( artistName );
		}

		public void ArtistForgotten( string artistName ) {
		}

		public void ArtistMetadataRequested( string artistName ) {
			var metadata = GetOrCreateArtistStatus( artistName );
		}

		private DbArtistStatus GetOrCreateArtistStatus( string forArtist ) {
			DbArtistStatus	retValue = null;

			if( mDocumentStore != null ) {
				using( var session = mDocumentStore.OpenSession()) {
					retValue = session.Load<DbArtistStatus>( DbArtistStatus.FormatStatusKey( forArtist ));

					if( retValue == null ) {
						retValue = new DbArtistStatus { ArtistName = forArtist };

						session.Store( retValue );
						session.SaveChanges();
					}
				}
			}

			return( retValue );
		}
	}
}
