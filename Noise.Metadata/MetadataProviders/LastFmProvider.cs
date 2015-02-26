using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.MetadataProviders.LastFm;
using Noise.Metadata.MetadataProviders.LastFm.Rto;
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
		private readonly ILastFmClient		mLastFmClient;
		private readonly ILicenseManager	mLicenseManager;
		private readonly INoiseLog			mLog;
		private readonly bool				mHasNetworkAccess;
		private IDocumentStore				mDocumentStore;

		public	string		ProviderKey { get; private set; }

		public LastFmProvider( ILastFmClient client, ILicenseManager licenseManager, NoiseCorePreferences preferences, INoiseLog log ) {
			mLastFmClient = client;
			mLicenseManager = licenseManager;
			mLog = log;

			mHasNetworkAccess = preferences.HasNetworkAccess;

			ProviderKey = "LastFm";
		}

		public void Initialize( IDocumentStore documentStore ) {
			mDocumentStore = documentStore;

			try {
				var key = mLicenseManager.RetrieveKey( LicenseKeys.LastFm );

				Condition.Requires( key ).IsNotNull();
			}
			catch( Exception ex ) {
				mLog.LogException( "Retrieving LastFm api key", ex );
			}
		}

		public void Shutdown() {
			
		}

		public void UpdateArtist( string artistName ) {
			AsyncUpdateArtist( artistName ).Wait();
		}

		// Last.fm provides artist biography, genre, similar artists and top albums.
		private async Task AsyncUpdateArtist( string artistName ) {
			if( mHasNetworkAccess ) {
				try {
					var artistSearch = await mLastFmClient.ArtistSearch( artistName );

					if( artistSearch != null ) {
						var firstArtist = artistSearch.Artist.FirstOrDefault();

						if( firstArtist != null ) {
							var artistInfo = await mLastFmClient.GetArtistInfo( firstArtist.Name );
							var topAlbums = await mLastFmClient.GetTopAlbums( firstArtist.Name );
							var topTracks = await mLastFmClient.GetTopTracks( firstArtist.Name );

							if(( artistInfo != null ) &&
							   ( topAlbums != null ) &&
							   ( topTracks != null )) {
								UpdateArtist( artistName, artistInfo, topAlbums, topTracks );
							}
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "LastFm update failed for artist: {0}", artistName ), ex );
				}
			}
		}

		private void UpdateArtist( string artistName, LastFmArtistInfo artistInfo, LastFmTopAlbums topAlbums, LastFmTopTracks topTracks ) {
			try {
				using( var session = mDocumentStore.OpenSession()) {
					var strList = new List<string>();
					var artistBio = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( artistName )) ??
						            new DbArtistBiography { ArtistName = artistName };

					artistBio.SetMetadata( eMetadataType.Biography, artistInfo.Bio.Content );
					artistBio.SetMetadata( eMetadataType.YearFormed, artistInfo.Bio.YearFormed.ToString());

					if( artistInfo.Tags.Tag.Any()) {
						strList.AddRange( artistInfo.Tags.Tag.Select( tag => tag.Name.ToLower()));
						artistBio.SetMetadata( eMetadataType.Genre, strList );
					}

					strList.Clear();

					if( artistInfo.Similar.Artist.Any()) {
						strList.AddRange( artistInfo.Similar.Artist.Select( sim => sim.Name ));
						artistBio.SetMetadata( eMetadataType.SimilarArtists, strList );
					}

					strList.Clear();

					if( topAlbums.TopAlbums.Album.Any()) {
						strList.AddRange( topAlbums.TopAlbums.Album.Take( 5 ).Select( album => album.Name ));
						artistBio.SetMetadata( eMetadataType.TopAlbums, strList );
					}

					if( artistInfo.Image.Any()) {
						var image = artistInfo.Image.FirstOrDefault( i => i.Size.Equals( "large", StringComparison.InvariantCultureIgnoreCase )) ??
						            artistInfo.Image.FirstOrDefault();

						if( image != null ) {
							new ImageDownloader( image.Url, artistName, ArtistImageDownloadComplete );
						}
					}

					strList.Clear();

					if( topTracks.TopTracks.Track.Any()) {
						strList.AddRange(( from topTrack in topTracks.TopTracks.Track orderby topTrack.Listeners descending select topTrack.Name ).Take( 10 ));
						artistBio.SetMetadata( eMetadataType.TopTracks, strList );
					}

					session.Store( artistBio );
					session.SaveChanges();

					mLog.LogMessage( string.Format( "LastFm updated artist: {0}", artistName ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "LastFm update failed for artist: {0}", artistName ), ex );
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
				mLog.LogException( string.Format( "ImageDownload failed for artist: {0} ", artistName ), ex );
			}
		}
	}
}
