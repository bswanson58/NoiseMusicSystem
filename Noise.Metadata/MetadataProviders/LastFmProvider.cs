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

namespace Noise.Metadata.MetadataProviders {
	internal class LastFmProvider : IArtistMetadataProvider {
		private readonly ILastFmClient				mLastFmClient;
		private readonly IArtistStatusProvider		mStatusProvider;
		private readonly IArtistBiographyProvider	mBiographyProvider;
		private readonly ILogMetadata				mLog;
		private readonly bool						mHasNetworkAccess;

		public	string		ProviderKey { get; }

		public LastFmProvider( ILastFmClient client, ILicenseManager licenseManager, IArtistBiographyProvider biographyProvider, IArtistStatusProvider statusProvider,
                               NoiseCorePreferences preferences, ILogMetadata log ) {
			mLastFmClient = client;
			mBiographyProvider = biographyProvider;
			mStatusProvider = statusProvider;
			mLog = log;

			mHasNetworkAccess = preferences.HasNetworkAccess;

			ProviderKey = "LastFm";

            try {
                var key = licenseManager.RetrieveKey( LicenseKeys.LastFm );

                Condition.Requires( key ).IsNotNull();
            }
            catch( Exception ex ) {
                mLog.LogException( "Retrieving LastFm API key", ex );
            }
		}

		// Last.fm provides artist biography, genre, similar artists and top albums.
		public async Task<bool> UpdateArtist( string artistName ) {
			var retValue = false;

			if( mHasNetworkAccess ) {
				try {
					var artistSearch = await mLastFmClient.ArtistSearch( artistName );

					if(( artistSearch?.ArtistList != null ) &&
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

							retValue = true;
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
					retValue = false;

					mLog.LogException( $"LastFm search failed for artist \"{artistName}\"", ex );
				}
			}

			return( retValue );
		}

		private void UpdateArtist( string artistName, LastFmArtistInfo artistInfo, LastFmAlbumList topAlbums, LastFmTrackList topTracks ) {
			try {
				var strList = new List<string>();
				var artistBio = mBiographyProvider.GetBiography( artistName ) ?? new DbArtistBiography { ArtistName = artistName };

				artistBio.SetMetadata( eMetadataType.Biography, artistInfo.Bio.Content );

				if(( artistInfo.Bio.FormationList?.Formation != null ) && 
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

/* LastFM artwork looks to be always a blank image with a star.
				if( artistInfo.ImageList.Any()) {
					var image = artistInfo.ImageList.FirstOrDefault( i => i.Size.Equals( "mega", StringComparison.InvariantCultureIgnoreCase )) ??
						        artistInfo.ImageList.FirstOrDefault();

					if(( image != null ) &&
					   (!string.IsNullOrWhiteSpace( image.Url ))) {
						ImageDownloader.StartDownload( image.Url, artistName, ArtistImageDownloadComplete );
					}
				}
*/
				if(!mBiographyProvider.InsertOrUpdate( artistBio )) {
                    mLog.LogException( "LastFM: Failed to update artist biography", new ApplicationException( "InsertOrUpdate Biography" ));
                }
			}
			catch( Exception ex ) {
				mLog.LogException( $"LastFm update failed for artist: {artistName}", ex );
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
				Stream	streamData = new MemoryStream( imageData );

				streamData.Seek( 0, SeekOrigin.Begin );
				mStatusProvider.PutArtistArtwork( artistName, streamData );
			}
			catch( Exception ex ) {
				mLog.LogException( $"ImageDownload failed for artist: {artistName} ", ex );
			}
		}
	}
}
