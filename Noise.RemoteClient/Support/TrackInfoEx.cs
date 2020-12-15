using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Support {
    static class TrackInfoEx {
        public static int GetRatingSort( this TrackInfo info ) {
            return info.IsFavorite ? 6 : 0 + info.Rating;
        }
    }
}
