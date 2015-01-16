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

		public DiscogsClient( ILicenseManager licenseManager ) {
			mLicenseManager = licenseManager;
		}

		private string RetrieveLicenseKey() {
			if( string.IsNullOrWhiteSpace( mAccessToken ) ) {
				var licenseKey = mLicenseManager.RetrieveKey( LicenseKeys.Discogs );

				mAccessToken = licenseKey.Key;
			}

			return( mAccessToken );
		}

		public async Task<ArtistSearchResult[]> ArtistSearch( string artistName ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var searchResults = await discogsApi.Search( artistName, cSearchItemTypeArtist, RetrieveLicenseKey());

			return( searchResults.Results );
		}

		public async Task<DiscogsArtist> GetArtist( string artistId ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var artist = await discogsApi.GetArtist( artistId, RetrieveLicenseKey());

			return( artist );
		}

		public async Task<DiscogsRelease[]> GetArtistReleases( string artistId ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var releases = await discogsApi.GetArtistReleases( artistId, RetrieveLicenseKey());

			return( releases.Releases );
		}
	}
}
