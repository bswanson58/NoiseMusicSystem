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

				mApiKey = licenseKey.Name;
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
			var searchResults = await LastFmApi.ArtistSearch( artistName, RetrieveLicenseKey(), cResultFormat );

			return ( searchResults.Results.ArtistMatches );
		}

		public async Task<LastFmArtistInfo> GetArtistInfo( string artistName ) {
			var results = await LastFmApi.GetArtistInfo( artistName, RetrieveLicenseKey(), cResultFormat );

			return ( results.Artist );
		}

		public async Task<LastFmTopAlbums> GetTopAlbums( string artistName ) {
			var results = await LastFmApi.GetTopAlbums( artistName, RetrieveLicenseKey(), cResultFormat );

			return ( results );
		}

		public async Task<LastFmTopTracks> GetTopTracks( string artistName ) {
			var results = await LastFmApi.GetTopTracks( artistName, RetrieveLicenseKey(), cResultFormat );

			return ( results );
		}
	}
}
