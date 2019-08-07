using System.Collections.Generic;

namespace Noise.Infrastructure.Dto {
    public enum eTrackPlaySuggesters {
        Stop = 1,
        Replay = 2,
        PlayList = 3,
        PlayArtist = 4,
        PlaySimilar = 5,
        PlayFavorites = 6,
        PlayGenre = 7,
        PlayArtistGenre = 8,
        PlayCategory = 9,
        SeldomPlayedArtists = 10,
        PlayUserTags = 11
    }

    public enum eTrackPlayDisqualifiers {
        ShortTracks = 1,
        AlreadyQueuedTracks = 2,
        BadlyRatedTracks = 3,
        DoNotPlayTracks = 4
    }

    public enum eTrackPlayBonus {
        HighlyRatedTracks = 1,
        PlayNextTrack = 2
    }

    public class ExhaustedStrategySpecification {
        public  IEnumerable<eTrackPlaySuggesters>       TrackSuggesters { get; }
        public  IEnumerable<eTrackPlayDisqualifiers>    TrackDisqualifiers { get; }
        public  IEnumerable<eTrackPlayBonus>            TrackBonusSuggesters { get; }

        public ExhaustedStrategySpecification() {
            TrackSuggesters = new List<eTrackPlaySuggesters>();
            TrackDisqualifiers = new List<eTrackPlayDisqualifiers>();
            TrackBonusSuggesters = new List<eTrackPlayBonus>();
        }
    }
}
