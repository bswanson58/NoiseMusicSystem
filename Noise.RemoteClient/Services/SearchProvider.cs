using System;
using System.Threading.Tasks;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class SearchProvider : BaseProvider<SearchInformation.SearchInformationClient>, ISearchProvider {
        private readonly IPlatformLog   mLog;

        public SearchProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log )
            : base( serviceLocator, hostProvider ) {
            mLog = log;
        }

        public async Task<SearchResponse> Search( string searchTerm ) {
            var client = Client;

            if( client != null ) {
                try {
                    return await client.SearchAsync( new SearchRequest{ SearchTerm = searchTerm });
                }
                catch( Exception ex ) {
                    mLog.LogException( "Search", ex );
                }
            }

            return default;
        }
    }
}
