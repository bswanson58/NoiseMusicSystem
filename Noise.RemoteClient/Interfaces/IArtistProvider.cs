using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface IArtistProvider {
        Task<ArtistListResponse>    GetArtistList();
    }
}
