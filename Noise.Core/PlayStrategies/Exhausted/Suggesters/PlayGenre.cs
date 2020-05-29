using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.Suggesters {
    public class PlayGenre : ExhaustedHandlerBase {
        private readonly IArtistProvider	mArtistProvider;
        private readonly IAlbumProvider     mAlbumProvider;
        private readonly ITrackProvider     mTrackProvider;
        private readonly IGenreProvider		mGenreProvider;

        public PlayGenre( IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IGenreProvider genreProvider ) :
            base( eTrackPlayHandlers.PlayGenre, eTrackPlayStrategy.Suggester, "Play Genre... ", "play random tracks from the chosen genre" ) {
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mGenreProvider = genreProvider;

            RequiresParameters = true;
        }

        public override void InitialConfiguration( ExhaustedStrategySpecification specification ) {
            var genre = ( from g in mGenreProvider.GetGenreList().List where g.DbId == specification.SuggesterParameter select g ).FirstOrDefault();
            if( genre != null ) {
                SetDescription( $"play random tracks from the '{ genre.Name }' genre" );
            }
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            using( var artistList = mArtistProvider.GetArtistList()) {
                var artistsFromGenre = ( from a in artistList.List where (( a.Genre == context.SuggesterParameter ) && ( a.Rating >= 0 )) select a ).ToList();
                var artist = artistsFromGenre.Skip( NextRandom( artistsFromGenre.Count )).FirstOrDefault();

                if( artist != null ) {
                    using( var albumList = mAlbumProvider.GetAlbumList( artist )) {
                        var album = albumList?.List.Skip( NextRandom( albumList.List.Count())).FirstOrDefault();

                        if( album != null ) {
                            using( var trackList = mTrackProvider.GetTrackList( album )) {
                                AddSuggestedTrack( SelectRandomTrack( trackList.List ), context );
                            }
                        }
                    }
                }
            }
        }
    }
}
