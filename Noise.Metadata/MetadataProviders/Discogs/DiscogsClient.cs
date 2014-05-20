﻿using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Refit;

namespace Noise.Metadata.MetadataProviders.Discogs {
	public class DiscogsClient : IDiscogsClient {
		private const string	cDiscogsUrl = "http://api.discogs.com";
		public const string		cSearchItemTypeArtist = "artist";

		public async Task<ArtistSearchResult[]> ArtistSearch( string artistName ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var searchResults = await discogsApi.Search( artistName, cSearchItemTypeArtist );

			return( searchResults.Results );
		}

		public async Task<DiscogsArtist> GetArtist( string artistId ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var artist = await discogsApi.GetArtist( artistId );

			return( artist );
		}

		public async Task<DiscogsRelease[]> GetArtistReleases( string artistId ) {
			var discogsApi = RestService.For<IDiscogsApi>( cDiscogsUrl );
			var releases = await discogsApi.GetArtistReleases( artistId );

			return( releases.Releases );
		}
	}
}