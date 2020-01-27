using Noise.Infrastructure.Dto;

namespace Noise.UI.Dto {
    public class UiTrackAlbum {
        private readonly DbAlbum    mAlbum;
        private readonly DbTrack    mTrack;

        public  string              TrackName => mTrack.Name;
        public  int                 SortRating => mTrack.IsFavorite ? 6 : mTrack.Rating;

        public UiTrackAlbum( DbAlbum album, DbTrack track ) {
            mAlbum = album;
            mTrack = track;
        }
    }
}
