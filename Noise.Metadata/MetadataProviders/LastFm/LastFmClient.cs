using System;
using System.Threading.Tasks;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.MetadataProviders.LastFm.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.LastFm {
	internal class LastFmClient : ILastFmClient {
		private const string cDiscogsUrl = "http://ws.audioscrobbler.com";
		private const string cResultFormat = "json";

		private readonly ILicenseManager mLicenseManager;
		private string mApiKey;
		private ILastFmApi mLastFmApi;

		public LastFmClient( ILicenseManager licenseManager ) {
			mLicenseManager = licenseManager;
		}

		private string RetrieveLicenseKey() {
			if( string.IsNullOrWhiteSpace( mApiKey ) ) {
				var licenseKey = mLicenseManager.RetrieveKey( LicenseKeys.LastFm );

				mApiKey = licenseKey.Key;
			}

			return ( mApiKey );
		}

		private ILastFmApi LastFmApi {
			get {
				if( mLastFmApi == null ) {
					mLastFmApi = RestService.For<ILastFmApi>( cDiscogsUrl );
				}

				return ( mLastFmApi );
			}
		}

		public async Task<LastFmArtistList> ArtistSearch( string artistName ) {
			var results = await LastFmApi.ArtistSearch( artistName, RetrieveLicenseKey(), cResultFormat );

			if( results == null ) {
				throw new ApplicationException( string.Format( "LastFm returned null search for artist '{0}'", artistName ));
			}
			if(( results.Results == null ) ||
			   ( results.Error != 0 )) {
				throw new ApplicationException( string.Format( "LastFm returned error code: {0} - {1}", results.Error, results.Message ));
			}

			// Push the count down into the returned value.
			if( results.Results.ArtistMatches != null ) {
				results.Results.ArtistMatches.TotalResults = results.Results.TotalResults;
			}

			return ( results.Results.ArtistMatches );
		}

		public async Task<LastFmArtistInfo> GetArtistInfo( string artistName ) {
			var results = await LastFmApi.GetArtistInfo( artistName, RetrieveLicenseKey(), cResultFormat );

			if( results == null ) {
				throw new ApplicationException( string.Format( "LastFm returned null info for artist '{0}'", artistName ));
			}
			if(( results.Artist == null ) ||
			   ( results.Error != 0 )) {
				throw new ApplicationException( string.Format( "LastFm returned error code: {0} - {1}", results.Error, results.Message ));
			}

			return ( results.Artist );
		}

		public async Task<LastFmAlbumList> GetTopAlbums( string artistName ) {
			var results = await LastFmApi.GetTopAlbums( artistName, RetrieveLicenseKey(), cResultFormat );

			if( results == null ) {
				throw new ApplicationException( string.Format( "LastFm returned null albumsList for artist '{0}'", artistName ));
			}
			if(( results.TopAlbums == null ) ||
			   ( results.Error != 0 )) {
				throw new ApplicationException( string.Format( "LastFm returned error code: {0} - {1}", results.Error, results.Message ));
			}

			return ( results.TopAlbums );
		}

		public async Task<LastFmTrackList> GetTopTracks( string artistName ) {
			var results = await LastFmApi.GetTopTracks( artistName, RetrieveLicenseKey(), cResultFormat );

			if( results == null ) {
				throw new ApplicationException( string.Format( "LastFm returned null TopTracksList for artist '{0}'", artistName ));
			}
			if(( results.TopTracks == null ) ||
			   ( results.Error != 0 )) {
				throw new ApplicationException( string.Format( "LastFm returned error code: {0} - {1}", results.Error, results.Message ));
			}

			return ( results.TopTracks );
		}
	}
}
