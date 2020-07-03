using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class RatedTracks : ExhaustedHandlerBase {
        private readonly ITrackProvider     mTrackProvider;

        public RatedTracks( ITrackProvider trackProvider ) :
            base( eTrackPlayHandlers.RatedTracks, eTrackPlayStrategy.Suggester, "Play Rated Tracks", "play top rated tracks" ) {
            mTrackProvider = trackProvider;

            RequiresParameters = true;
        }

        public override void InitialConfiguration( ExhaustedStrategySpecification specification ) {
            SetDescription( $"play tracks rated with {specification.SuggesterParameter} stars or greater" );
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var rating = context.SuggesterParameter;

            using( var ratedTracks = mTrackProvider.GetRatedTracks((int)rating )) {
                AddSuggestedTrack( SelectRandomTrack( ratedTracks.List ), context );
            }
        }
    }
}
