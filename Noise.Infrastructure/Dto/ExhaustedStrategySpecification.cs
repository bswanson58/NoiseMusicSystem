using System.Collections.Generic;
using System.Linq;

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
        Replay = 101,
        PlayArtist = 102,
        PlaySimilar = 103,
        PlayFavorites = 104,
        PlayGenre = 105,
        SeldomPlayedArtists = 106,
        PlayUserTags = 107,
        RatedTracks = 108,

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

                retValue.TrackSuggesters.Add( eTrackPlayHandlers.Stop );

                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.AlreadyQueuedTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.ShortTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.BadRatingTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.TalkingTracks );
                retValue.TrackDisqualifiers.Add( eTrackPlayHandlers.DoNotPlayTracks );

//                retValue.TrackBonusSuggesters.Add( eTrackPlayHandlers.HighlyRatedTracks );
//                retValue.TrackBonusSuggesters.Add( eTrackPlayHandlers.PlayAdjacentTracks );

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

        public eTrackPlayHandlers PrimarySuggester() {
            var retValue = eTrackPlayHandlers.Unknown;

            if( TrackSuggesters.Any()) {
                retValue = TrackSuggesters.FirstOrDefault();
            }

            return retValue;
        }
    }
}
