using System.Collections.Generic;

namespace Noise.Infrastructure.Dto {
    public enum eTrackPlayStrategy {
        Suggester,
        Disqualifier,
        BonusSuggester
    }

    public enum eTrackPlayHandlers {
        // Suggesters
        Unknown = 0,
        Stop = 100,
        Replay = 102,
        PlayList = 103,
        PlayArtist = 104,
        PlaySimilar = 105,
        PlayFavorites = 106,
        PlayGenre = 107,
        PlayArtistGenre = 108,
        PlayCategory = 109,
        SeldomPlayedArtists = 110,
        PlayUserTags = 111,

        // Disqualifiers
        ShortTracks = 201,
        AlreadyQueuedTracks = 202,
        BadlyRatedTracks = 203,
        DoNotPlayTracks = 204,

        // Bonus handlers
        HighlyRatedTracks = 301,
        PlayNextTrack = 302
    }

    public class ExhaustedStrategySpecification {
        public  IList<eTrackPlayHandlers>   TrackSuggesters { get; }
        public  IList<eTrackPlayHandlers>   TrackDisqualifiers { get; }
        public  IList<eTrackPlayHandlers>   TrackBonusSuggesters { get; }
        public  long                        SuggesterParameter { get; set; }

        public static ExhaustedStrategySpecification    Default {
            get {
                var retValue = new ExhaustedStrategySpecification();

                retValue.TrackSuggesters.Add( eTrackPlayHandlers.PlayFavorites );

                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.AlreadyQueuedTracks );

                return retValue;
            }
        }

        public ExhaustedStrategySpecification() {
            TrackSuggesters = new List<eTrackPlayHandlers>();
            TrackDisqualifiers = new List<eTrackPlayHandlers>();
            TrackBonusSuggesters = new List<eTrackPlayHandlers>();

            SuggesterParameter = Constants.cDatabaseNullOid;
        }

        public ExhaustedStrategySpecification( ExhaustedStrategySpecification copy ) {
            TrackSuggesters = new List<eTrackPlayHandlers>( copy.TrackSuggesters );
            TrackDisqualifiers = new List<eTrackPlayHandlers>( copy.TrackDisqualifiers );
            TrackBonusSuggesters = new List<eTrackPlayHandlers>( copy.TrackBonusSuggesters );
            SuggesterParameter = copy.SuggesterParameter;
        }

        public void SetPrimarySuggester( eTrackPlayHandlers suggester ) {
            TrackSuggesters.Clear();
            TrackSuggesters.Add( suggester );
        }
    }
}
