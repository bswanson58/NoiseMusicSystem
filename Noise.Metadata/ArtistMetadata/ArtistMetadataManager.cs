﻿using System.Collections.Generic;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;

namespace Noise.Metadata.ArtistMetadata {
	class ArtistMetadataManager : IArtistMetadataManager {
        private readonly IArtistArtworkSelector mArtworkSelector;
		private IDocumentStore				    mDocumentStore;

        public ArtistMetadataManager( IArtistArtworkSelector artworkSelector ) {
            mArtworkSelector = artworkSelector;
        }

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

        public bool ArtistPortfolioAvailable( string forArtist ) {
            return mArtworkSelector.ArtistPortfolioAvailable( forArtist );
        }

        public IArtistMetadata GetArtistBiography( string forArtist ) {
			return( GetOrCreateArtistBiography( forArtist ));
		}

		public IArtistDiscography GetArtistDiscography( string forArtist ) {
			return( GetOrCreateArtistDiscography( forArtist ));
		}

		public Artwork GetArtistArtwork( string forArtist ) {
			var	retValue = new Artwork( new DbArtwork( Constants.cDatabaseNullOid, ContentType.ArtistPrimaryImage ));

            mArtworkSelector.SelectArtwork( forArtist, retValue );
            if(!retValue.HaveValidImage ) {
                var attachment = mDocumentStore.DatabaseCommands.GetAttachment( "artwork/" + forArtist.ToLower());

                if( attachment != null ) {
                    retValue.Image = new byte[attachment.Size];

                    attachment.Data().Read( retValue.Image, 0, attachment.Size );
                }
            }

			return( retValue );
		}

        public IEnumerable<Artwork> GetArtistPortfolio( string forArtist ) {
            return mArtworkSelector.ArtworkPortfolio( forArtist );
        }

        private void InsureArtistStatus( string forArtist ) {
			if(( mDocumentStore != null ) &&
			   (!mDocumentStore.WasDisposed )) {
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

			if(( mDocumentStore != null ) &&
			   (!mDocumentStore.WasDisposed )) {
				using( var session = mDocumentStore.OpenSession() ) {
					retValue = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( forArtist ));
				}
			}

			if( retValue == null ) {
				retValue = new DbArtistBiography { ArtistName = forArtist };
			}

			return( retValue );
		}

		private DbArtistDiscography GetOrCreateArtistDiscography( string forArtist ) {
			var retValue = default( DbArtistDiscography );

			if(( mDocumentStore != null ) &&
			   (!mDocumentStore.WasDisposed )) {
				using( var session = mDocumentStore.OpenSession() ) {
					retValue = session.Load<DbArtistDiscography>( DbArtistDiscography.FormatStatusKey( forArtist ));
				}
			}

			if( retValue == null ) {
				retValue = new DbArtistDiscography { ArtistName = forArtist };
			}

			return( retValue );
		}
	}
}
