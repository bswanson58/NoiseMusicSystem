using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class SearchProvider : BaseProvider<SearchInformation.SearchInformationClient>, ISearchProvider {
        public SearchProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider )
            : base( serviceLocator, hostProvider ) { }

        public async Task<SearchResponse> Search( string searchTerm ) {
            var client = Client;

            if( client != null ) {
                return await client.SearchAsync( new SearchRequest{ SearchTerm = searchTerm });
            }

            return default;
        }
    }
}
