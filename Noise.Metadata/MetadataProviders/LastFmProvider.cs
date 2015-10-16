using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;
using Noise.Metadata.Logging;
using Noise.Metadata.MetadataProviders.LastFm;
using Noise.Metadata.MetadataProviders.LastFm.Rto;
using Raven.Client;
using Raven.Json.Linq;

namespace Noise.Metadata.MetadataProviders {
	internal class LastFmProvider : IArtistMetadataProvider {
		private readonly ILastFmClient		mLastFmClient;
		private readonly ILicenseManager	mLicenseManager;
		private readonly ILogMetadata		mLog;
		private readonly bool				mHasNetworkAccess;
		private IDocumentStore				mDocumentStore;

		public	string		ProviderKey { get; private set; }

		public LastFmProvider( ILastFmClient client, ILicenseManager licenseManager, NoiseCorePreferences preferences, ILogMetadata log ) {
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

					if(( artistSearch != null ) &&
					   (artistSearch.ArtistList != null ) &&
					   ( artistSearch.ArtistList.Count > 0 )) {
						var firstArtist = artistSearch.ArtistList.FirstOrDefault();

						if( firstArtist != null ) {
							var artistInfoTask = mLastFmClient.GetArtistInfo( firstArtist.Name );
							var topAlbumsTask = mLastFmClient.GetTopAlbums( firstArtist.Name );
							var topTracksTask = mLastFmClient.GetTopTracks( firstArtist.Name );

							// Run all tasks in parallel and wait for them all to finish.
							await Task.WhenAll( artistInfoTask, topAlbumsTask, topTracksTask );

							var artistInfo = await artistInfoTask;
							var topAlbums = await topAlbumsTask;
							var topTracks = await topTracksTask;

							if(( artistInfo != null ) &&
							   ( topAlbums != null ) &&
							   ( topTracks != null )) {
								UpdateArtist( artistName, artistInfo, topAlbums, topTracks );

								mLog.LoadedMetadata( ProviderKey, artistName );
							}
						}
						else {
							mLog.ArtistNotFound( ProviderKey, artistName );
						}
					}
					else {
						mLog.ArtistNotFound( ProviderKey, artistName );
					}
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "LastFm search failed for artist \"{0}\"", artistName ), ex );
				}
			}
		}

		private void UpdateArtist( string artistName, LastFmArtistInfo artistInfo, LastFmAlbumList topAlbums, LastFmTrackList topTracks ) {
			try {
				using( var session = mDocumentStore.OpenSession()) {
					var strList = new List<string>();
					var artistBio = session.Load<DbArtistBiography>( DbArtistBiography.FormatStatusKey( artistName )) ??
						            new DbArtistBiography { ArtistName = artistName };

					artistBio.SetMetadata( eMetadataType.Biography, artistInfo.Bio.Content );

					if(( artistInfo.Bio.FormationList != null ) &&
					   ( artistInfo.Bio.FormationList.Formation != null ) &&
					   ( artistInfo.Bio.FormationList.Formation.Any())) {
						UpdateArtistFormation( artistBio, artistInfo.Bio.FormationList.Formation );
					}

					if( artistInfo.Tags.TagList.Any()) {
						strList.AddRange( artistInfo.Tags.TagList.Select( tag => tag.Name.ToLower()));
						artistBio.SetMetadata( eMetadataType.Genre, strList );
					}

					strList.Clear();

					if( artistInfo.Similar.ArtistList.Any()) {
						strList.AddRange( artistInfo.Similar.ArtistList.Select( sim => sim.Name ));
						artistBio.SetMetadata( eMetadataType.SimilarArtists, strList );
					}

					strList.Clear();

					if( topAlbums.AlbumList.Any()) {
						strList.AddRange( topAlbums.AlbumList.Take( 5 ).Select( album => album.Name ));
						artistBio.SetMetadata( eMetadataType.TopAlbums, strList );
					}

					strList.Clear();

					if( topTracks.TrackList.Any()) {
						strList.AddRange(( from topTrack in topTracks.TrackList orderby topTrack.Listeners descending select topTrack.Name ).Take( 10 ));
						artistBio.SetMetadata( eMetadataType.TopTracks, strList );
					}

					if( artistInfo.ImageList.Any()) {
						var image = artistInfo.ImageList.FirstOrDefault( i => i.Size.Equals( "large", StringComparison.InvariantCultureIgnoreCase )) ??
						            artistInfo.ImageList.FirstOrDefault();

						if(( image != null ) &&
						   (!string.IsNullOrWhiteSpace( image.Url ))) {
							ImageDownloader.StartDownload( image.Url, artistName, ArtistImageDownloadComplete );
						}
					}

					session.Store( artistBio );
					session.SaveChanges();
				}
			}
			catch( Exception ex ) {
				mLog.LogException( string.Format( "LastFm update failed for artist: {0}", artistName ), ex );
			}
		}

		private void UpdateArtistFormation( DbArtistBiography artistBio, IEnumerable<LastFmFormation> formationList ) {
			var strFormation = new StringBuilder();

			foreach( var formation in formationList ) {
				if( strFormation.Length > 0 ) {
					strFormation.Append( ", " );
				}

				strFormation.Append( string.Format( "{0} - {1}", formation.YearFrom == 0 ? "Unknown" : formation.YearFrom.ToString( CultureInfo.InvariantCulture ),
																 formation.YearTo == 0 ? "Present" : formation.YearTo.ToString( CultureInfo.InvariantCulture )));
			}

			if( strFormation.Length > 0 ) {
				artistBio.SetMetadata( eMetadataType.ActiveYears, strFormation.ToString());
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
