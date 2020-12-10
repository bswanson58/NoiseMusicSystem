using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface ITrackProvider {
        Task<TrackListResponse>     GetTrackList( long artistId, long albumId );
    }
}
