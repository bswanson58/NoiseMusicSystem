using System;
using System.Collections.Generic;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.Metadata.Interfaces;
using Raven.Client;

namespace Noise.Metadata.MetadataProviders {
	internal class LastFmProvider : IArtistMetadataProvider {
		private Session			mSession;
		private IDocumentStore	mDocumentStore;

		public	string		ProviderKey { get; private set; }

		public LastFmProvider() {
			ProviderKey = "LastFm";
		}

		public void Initialize( IDocumentStore documentStore ) {
			try {
				var key = NoiseLicenseManager.Current.RetrieveKey( LicenseKeys.LastFm );

				if( key != null ) {
					mSession = new Session( key.Name, key.Key );
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LastFmProvider creating Session:", ex );
			}
		}

		public void Shutdown() {
			
		}

		public void UpdateArtist( string artistName ) {
			if( mSession != null ) {
				try {
					var	artistSearch = Artist.Search( artistName, mSession );
					var	artistMatch = artistSearch.GetFirstMatch();

					if( artistMatch != null ) {
						var	tags = artistMatch.GetTopTags( 3 );
						if( tags.GetLength( 0 ) > 0 ) {
						}

						var imageUrl = artistMatch.GetImageURL();
						if(!string.IsNullOrWhiteSpace( imageUrl )) {
						}

						var	sim = artistMatch.GetSimilar( 5 );
						var artistList = new List<string>();
						for( int index = 0; index < sim.GetLength( 0 ); index++ ) {
							artistList.Add( sim[index].Name );
						}

						var top = artistMatch.GetTopAlbums();
						var albumList = new List<string>();
						for( int index = 0; index < ( top.GetLength( 0 ) > 5 ? 5 : top.GetLength( 0 )); index++ ) {
							albumList.Add( top[index].Item.Name );
						}

						NoiseLogger.Current.LogMessage( "LastFm updated artist: {0}", artistName );
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LastFmProvider:UpdateArtistInfo:", ex );
				}
			}
		}

		private static void ArtistImageDownloadComplete( IArtworkProvider artworkProvider, long artworkId, byte[] imageData ) {
			Condition.Requires( artworkProvider ).IsNotNull();
			Condition.Requires( imageData ).IsNotNull();

			try {
				using( var updater = artworkProvider.GetArtworkForUpdate( artworkId )) {
					if( updater.Item != null ) {
						updater.Item.Image = imageData;
						updater.Item.UpdateExpiration();

						updater.Update();
					}
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LastFmProvider:ImageDownload: ", ex );
			}
		}
	}
}
