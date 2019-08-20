using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.BonusSuggesters {
    class PlayAdjacentTrack : ExhaustedHandlerBase {
        private readonly ITrackProvider     mTrackProvider;

        public PlayAdjacentTrack( ITrackProvider trackProvider )
            : base( eTrackPlayHandlers.PlayAdjacentTracks, eTrackPlayStrategy.BonusSuggester, "Play Adjacent Tracks", "Play indicated tracks adjacent to suggestions." ) {
            mTrackProvider = trackProvider;
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            bool tracksWereAdded;

            do {
                var trackList = context.SelectedTracks.ToList();

                tracksWereAdded = false;

                trackList.ForEach( 
                    track => {
                        if(( track.PlayStrategyOptions == ePlayAdjacentStrategy.PlayNext ) ||
                           ( track.PlayStrategyOptions == ePlayAdjacentStrategy.PlayNextPrevious )) {
                            tracksWereAdded |= CheckNext( track, context );
                        }
                    });

                trackList.ForEach( 
                    track => {
                        if(( track.PlayStrategyOptions == ePlayAdjacentStrategy.PlayPrevious ) ||
                           ( track.PlayStrategyOptions == ePlayAdjacentStrategy.PlayNextPrevious )) {
                            tracksWereAdded |= CheckPrevious( track, context );
                        }
                    });
            } while( tracksWereAdded );
        }

        private bool CheckNext( DbTrack track, IExhaustedSelectionContext context ) {
            var retValue = false;

            using( var trackList = mTrackProvider.GetTrackList( track.Album )) {
                var sortedList = from t in trackList.List orderby t.VolumeName, t.TrackNumber select t;
                var followingTrack = sortedList.SkipWhile( t => !t.DbId.Equals( track.DbId )).Skip( 1 ).FirstOrDefault();

                if( followingTrack != null ) {
                    var nextQueuedTrack = context.SelectedTracks.SkipWhile( t => !t.DbId.Equals( followingTrack.DbId )).FirstOrDefault();

                    if(( nextQueuedTrack == null ) ||
                       (!nextQueuedTrack.DbId.Equals( followingTrack.DbId ))) {
                        if( CanSuggestTrack( followingTrack, context )) {
                            context.SelectedTracks.Insert( context.SelectedTracks.IndexOf( track ) + 1, followingTrack );

                            retValue = true;
                        }
                    }
                }
            }

            return retValue;
        }

        private bool CheckPrevious( DbTrack track, IExhaustedSelectionContext context ) {
            var retValue = false;

            using( var trackList = mTrackProvider.GetTrackList( track.Album )) {
                var sortedList = from t in trackList.List orderby t.VolumeName, t.TrackNumber select t;
                var previousTrack = sortedList.Reverse().SkipWhile( t => !t.DbId.Equals( track.DbId )).Skip( 1 ).FirstOrDefault();

                if( previousTrack != null ) {
                    var previousQueuedTrack = context.SelectedTracks.Reverse().SkipWhile( t => !t.DbId.Equals( previousTrack.DbId )).FirstOrDefault();

                    if(( previousQueuedTrack == null ) ||
                       (!previousQueuedTrack.DbId.Equals( previousTrack.DbId ))) {
                        if( CanSuggestTrack( previousTrack, context )) {
                            context.SelectedTracks.Insert( context.SelectedTracks.IndexOf( track ), previousTrack );

                            retValue = true;
                        }
                    }
                }
            }

            return retValue;
        }
    }
}
