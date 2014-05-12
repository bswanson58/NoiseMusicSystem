using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CuttingEdge.Conditions;
using Lastfm.Services;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Raven.Client;
using Raven.Json.Linq;

namespace Noise.Metadata.MetadataProviders {
	internal class ImageDownloader {
		private readonly WebClient			mWebClient;
		private readonly string				mArtistName;
		private readonly Action< string, byte[]>	mOnDownloadComplete;

		public ImageDownloader( string uriString, string artistName, Action<string, byte[]> onCompleted ) {
			mArtistName = artistName;
			mWebClient = new WebClient();
			mOnDownloadComplete = onCompleted;

			mWebClient.DownloadDataCompleted += OnDownloadCompleted;
			mWebClient.DownloadDataAsync( new Uri( uriString ));
		}

		private void OnDownloadCompleted( object sender, DownloadDataCompletedEventArgs e ) {
			if(( e.Error == null ) &&
			   ( e.Cancelled == false )) {
				mOnDownloadComplete( mArtistName, e.Result );
			}			

			mWebClient.DownloadDataCompleted -= OnDownloadCompleted;
		}
	}

	internal class LastFmProvider : IArtistMetadataProvider {
		private Session			mSession;
		private IDocumentStore	mDocumentStore;
		private bool			mHasNetworkAccess;

		public	string		ProviderKey { get; private set; }

		public LastFmProvider() {
			ProviderKey = "LastFm";
		}

		public void Initialize( IDocumentStore documentStore, ILicenseManager licenseManager ) {
			mDocumentStore = documentStore;

			try {
				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
				if( configuration != null ) {
					mHasNetworkAccess = configuration.HasNetworkAccess;

					var key = licenseManager.RetrieveKey( LicenseKeys.LastFm );

					Condition.Requires( key ).IsNotNull();

					if( key != null ) {
						mSession = new Session( key.Name, key.Key );
					}

					Condition.Requires( mSession ).IsNotNull();
				}

			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LastFmProvider:Initialize", ex );
			}
		}

		public void Shutdown() {
			
		}

		// Last.fm provides artist biography, genre, similar artists and top albums.
		public void UpdateArtist( string artistName ) {
			if(( mHasNetworkAccess ) &&
			   ( mSession != null )) {
				try {
					using( var session = mDocumentStore.OpenSession()) {
						var artistBio = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( artistName )) ??
						                new DbArtistBiography { ArtistName = artistName };

						var	artistSearch = Artist.Search( artistName, mSession );
						var	artistMatch = artistSearch.GetFirstMatch();

						if( artistMatch != null ) {
							var strList = new List<string>();

							artistBio.SetMetadata( eMetadataType.Biography, artistMatch.Bio.getContent());

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

							var imageUrl = artistMatch.GetImageURL();
							if(!string.IsNullOrWhiteSpace( imageUrl )) {
								new ImageDownloader( imageUrl, artistName, ArtistImageDownloadComplete );
							}

							strList.Clear();
							var topTracks = artistMatch.GetTopTracks();
							if( topTracks.GetLength( 0 ) > 0 ) {
								strList.AddRange(( from topTrack in topTracks orderby topTrack.Weight descending select topTrack.Item.Title ).Take( 10 ));
								artistBio.SetMetadata( eMetadataType.TopTracks, strList );
							}

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

		private void ArtistImageDownloadComplete( string artistName, byte[] imageData ) {
			Condition.Requires( imageData ).IsNotNull();

			try {
				if( mDocumentStore != null ) {
					Stream	streamData = new MemoryStream( imageData );

					mDocumentStore.DatabaseCommands.PutAttachment( "artwork/" + artistName.ToLower(), null, streamData, new RavenJObject());
				}
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( string.Format( "LastFmProvider:ImageDownload for artist: {0} ", artistName ), ex );
			}
		}
	}
}
