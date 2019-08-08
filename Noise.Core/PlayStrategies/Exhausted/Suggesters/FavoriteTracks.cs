using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class FavoriteTracks : ExhaustedHandlerBase {
        private readonly ITrackProvider     mTrackProvider;

        public FavoriteTracks( ITrackProvider trackProvider ) :
            base( eTrackPlaySuggesters.PlayFavorites ) {
            mTrackProvider = trackProvider;
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            using( var favoriteTracks = mTrackProvider.GetFavoriteTracks()) {
                context.SelectedTracks.Add( SelectRandomTrack( favoriteTracks.List ));
            }
        }
    }
}
