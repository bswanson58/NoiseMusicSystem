using System.Threading.Tasks;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Interfaces {
    interface ITrackProvider {
        Task<TrackListResponse>     GetTrackList( long artistId, long albumId );
        Task<TrackListResponse>     GetRatedTracks( long artistId, int includeRatingsOver, bool includeFavorites );
        Task<TrackListResponse>     GetTaggedTracks( long trackId );
        Task<TrackListResponse>     GetSimilarTracks( long trackId );
        Task<TrackListResponse>     GetFavoriteTracks();
        Task<TrackUpdateResponse>   UpdateTrackRatings( TrackInfo track );
    }
}
