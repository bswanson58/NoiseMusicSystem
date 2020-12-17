using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface ISearchProvider {
        Task<SearchResponse>    Search( string searchTerm );
    }
}
