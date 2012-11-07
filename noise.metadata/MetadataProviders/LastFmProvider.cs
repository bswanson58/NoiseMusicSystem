using System;
using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.Metadata.Dto;
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
			mDocumentStore = documentStore;

			try {
				var key = NoiseLicenseManager.Current.RetrieveKey( LicenseKeys.LastFm );

				Condition.Requires( key ).IsNotNull();

				if( key != null ) {
					mSession = new Session( key.Name, key.Key );
				}

				Condition.Requires( mSession ).IsNotNull();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LastFmProvider:Initialize", ex );
			}
		}

		public void Shutdown() {
			
		}

		// Last.fm provides artist genre, similar artists and top albums.
		public void UpdateArtist( string artistName ) {
			if( mSession != null ) {
				try {
					using( var session = mDocumentStore.OpenSession()) {
						var artistBio = session.Load<DbArtistBiography>( artistName );
						
						if( artistBio == null ) {
							artistBio = new DbArtistBiography { ArtistName = artistName };
						}

						var	artistSearch = Artist.Search( artistName, mSession );
						var	artistMatch = artistSearch.GetFirstMatch();

						if( artistMatch != null ) {
							var strList = new List<string>();

							var	tags = artistMatch.GetTopTags( 3 );
							if( tags.GetLength( 0 ) > 0 ) {
								strList.AddRange( tags.Select( tag => tag.Item.Name.ToLower()));
								artistBio.SetMetadata( eMetadataType.Genre, strList );
							}

							strList.Clear();
							var	similar = artistMatch.GetSimilar( 5 );
							if( similar.GetLength( 0 ) > 0 ) {
								strList.AddRange( similar.Select( sim => sim.Name ));
								artistBio.SetMetadata( eMetadataType.SimilarArtists, strList );
							}

							strList.Clear();
							var topAlbums = artistMatch.GetTopAlbums();
							if( topAlbums.GetLength( 0 ) > 0 ) {
								strList.AddRange( topAlbums.Take( 5 ).Select( album => album.Item.Name ));
								artistBio.SetMetadata( eMetadataType.TopAlbums, strList );
							}

//							var imageUrl = artistMatch.GetImageURL();
//							if(!string.IsNullOrWhiteSpace( imageUrl )) {
//							}

							session.Store( artistBio );
							session.SaveChanges();

							NoiseLogger.Current.LogMessage( "LastFm updated artist: {0}", artistName );
						}
					}
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( string.Format( "LastFm update failed for artist: {0}", artistName ), ex );
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
