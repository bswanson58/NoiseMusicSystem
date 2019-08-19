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
        PlayArtist = 103,
        PlaySimilar = 104,
        PlayFavorites = 105,
        PlayGenre = 106,
        SeldomPlayedArtists = 108,
        PlayUserTags = 109,

        // Disqualifiers
        ShortTracks = 201,
        AlreadyQueuedTracks = 202,
        BadRatingTracks = 203,
        TalkingTracks = 204,
        DoNotPlayTracks = 205,

        // Bonus handlers
        HighlyRatedTracks = 301,
        PlayAdjacentTracks = 302
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
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.ShortTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.BadRatingTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.TalkingTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.DoNotPlayTracks );

                retValue.TrackBonusSuggesters.Add( eTrackPlayHandlers.HighlyRatedTracks );
                retValue.TrackBonusSuggesters.Add( eTrackPlayHandlers.PlayAdjacentTracks );

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
