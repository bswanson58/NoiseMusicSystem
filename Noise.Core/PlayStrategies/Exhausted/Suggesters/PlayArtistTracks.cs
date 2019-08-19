using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    class PlayArtistTracks : ExhaustedHandlerBase {
        private readonly IArtistProvider    mArtistProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly ITrackProvider     mTrackProvider;

        public PlayArtistTracks( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
            base( eTrackPlayHandlers.PlayArtist, eTrackPlayStrategy.Suggester, "Play Artist...", "Play random tracks from the selected artist." ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;

            RequiresParameters = true;
        }

        public override void InitialConfiguration( ExhaustedStrategySpecification specification ) {
            var artist = mArtistProvider.GetArtist( specification.SuggesterParameter );

            if( artist != null ) {
                SetDescription( $"play tracks from artist '{artist.Name}'" );
            }
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            using( var albumList = mAlbumProvider.GetAlbumList( context.SuggesterParameter )) {
                if(( albumList.List.Any())) {
                    var album = albumList.List.Skip( NextRandom( albumList.List.Count())).FirstOrDefault();

                    if( album != null ) {
                        using( var trackList = mTrackProvider.GetTrackList( album )) {
                            AddSuggestedTrack( SelectRandomTrack( trackList.List ), context  );
                        }
                    }
                }
            }
        }
    }
}
