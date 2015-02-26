using System.Threading.Tasks;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.Discogs {
	public class DiscogsClient : IDiscogsClient {
		private const string				cDiscogsUrl = "https://api.discogs.com";
		public const string					cSearchItemTypeArtist = "artist";

		private readonly ILicenseManager	mLicenseManager;
		private string						mAccessToken;
		private IDiscogsApi					mDiscogsApi;

		public DiscogsClient( ILicenseManager licenseManager ) {
			mLicenseManager = licenseManager;
		}

		private string RetrieveLicenseKey() {
			if( string.IsNullOrWhiteSpace( mAccessToken )) {
				var licenseKey = mLicenseManager.RetrieveKey( LicenseKeys.Discogs );

				mAccessToken = licenseKey.Key;
			}

			return( mAccessToken );
		}

		private IDiscogsApi DiscogsApi {
			get {
				if( mDiscogsApi == null ) {
					mDiscogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
				}

				return( mDiscogsApi );
			}
		}

		public async Task<ArtistSearchResult[]> ArtistSearch( string artistName ) {
			var searchResults = await DiscogsApi.Search( artistName, cSearchItemTypeArtist, RetrieveLicenseKey());

			return( searchResults.Results );
		}

		public async Task<DiscogsArtist> GetArtist( string artistId ) {
			return( await DiscogsApi.GetArtist( artistId, RetrieveLicenseKey()));
		}

		public async Task<DiscogsRelease[]> GetArtistReleases( string artistId ) {
			var releases = await DiscogsApi.GetArtistReleases( artistId, RetrieveLicenseKey());

			return( releases.Releases );
		}
	}
}
