using System;
using System.Collections.Generic;
using System.IO;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;

namespace Noise.Metadata.ArtistMetadata {
	class ArtistMetadataManager : IArtistMetadataManager {
        private readonly IArtistArtworkSelector		mArtworkSelector;
		private readonly IArtistBiographyProvider	mBiographyProvider;
		private readonly IArtistDiscographyProvider	mDiscographyProvider;
		private readonly IArtistStatusProvider		mStatusProvider;
		private readonly ILogMetadata				mLog;

        public ArtistMetadataManager( IArtistArtworkSelector artworkSelector, IArtistBiographyProvider biographyProvider, IArtistDiscographyProvider discographyProvider,
                                      IArtistStatusProvider statusProvider, ILogMetadata log ) {
            mArtworkSelector = artworkSelector;
			mBiographyProvider = biographyProvider;
			mDiscographyProvider = discographyProvider;
			mStatusProvider = statusProvider;
			mLog = log;
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
				try {
                    using( var stream = new MemoryStream()) {
                        mStatusProvider.GetArtistArtwork( forArtist, stream );

						retValue.Image = new byte[stream.Length];
                        stream.Seek( 0, SeekOrigin.Begin );
                        stream.Read( retValue.Image, 0, (int)stream.Length );
                    }
                }
				catch( Exception ex ) {
					mLog.LogException( "GetArtistArtwork: Retrieving image from database", ex );
                }
            }

			return retValue;
		}

        public IEnumerable<Artwork> GetArtistPortfolio( string forArtist ) {
            return mArtworkSelector.ArtworkPortfolio( forArtist );
        }

        private void InsureArtistStatus( string forArtist ) {
			try {
                if( mStatusProvider.GetStatus( forArtist ) == null ) {
                    mStatusProvider.Insert( new DbArtistStatus() { ArtistName = forArtist });
                }
            }
			catch( Exception ex ) {
				mLog.LogException( nameof( InsureArtistStatus ), ex );
            }
		}

		private DbArtistBiography GetOrCreateArtistBiography( string forArtist ) {
			var retValue = default( DbArtistBiography );

			try {
                retValue = mBiographyProvider.GetBiography( forArtist ) ?? new DbArtistBiography { ArtistName = forArtist };
            }
			catch( Exception ex ) {
				mLog.LogException( nameof( GetOrCreateArtistBiography ), ex );
            }

			return retValue;
		}

		private DbArtistDiscography GetOrCreateArtistDiscography( string forArtist ) {
			var retValue = default( DbArtistDiscography );

            try {
                retValue = mDiscographyProvider.GetDiscography( forArtist ) ?? new DbArtistDiscography { ArtistName = forArtist };
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( GetOrCreateArtistDiscography ), ex );
            }

			return retValue;
		}
	}
}
