using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.ExtensionClasses.MoreLinq;

namespace Noise.Core.PlayStrategies.Exhausted.BonusSuggesters {
    public class PlayAdjacentTrack : ExhaustedHandlerBase {
        private readonly ITrackProvider     mTrackProvider;

        public PlayAdjacentTrack( ITrackProvider trackProvider )
            : base( eTrackPlayHandlers.PlayAdjacentTracks, eTrackPlayStrategy.BonusSuggester, "Play Adjacent Tracks", "Play indicated tracks adjacent to suggestions." ) {
            mTrackProvider = trackProvider;
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            var finalTracks = new List<DbTrack>();

            context.SelectedTracks.ForEach( t => {
                if( t.PlayAdjacentStrategy == ePlayAdjacentStrategy.None ) {
                    finalTracks.Add( t );
                }
                else {
                    finalTracks.AddRange( BuildAdjacentTracks( t ));
                }
            });

            context.SelectedTracks.Clear();
            finalTracks.ForEach( t => AddSuggestedTrack( t, context ));
        }

        private IEnumerable<DbTrack> BuildAdjacentTracks( DbTrack track ) {
            var retValue = new List<DbTrack>{ track };

            using( var trackList = mTrackProvider.GetTrackList( track.Album )) {
                var sortedList = ( from t in trackList.List orderby t.VolumeName descending, t.TrackNumber descending select t ).ToList();

                while(( retValue.FirstOrDefault()?.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayPrevious ) ||
                      ( retValue.FirstOrDefault()?.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious )) {
                    var previousTrack = sortedList.SkipWhile( t => !t.DbId.Equals( retValue.First().DbId )).Skip( 1 ).FirstOrDefault();

                    if( previousTrack != null ) {
                        retValue.Insert( 0, previousTrack );
                    }
                    else {
                        break;
                    }
                }

                sortedList = ( from t in trackList.List orderby t.VolumeName, t.TrackNumber select t ).ToList();

                while(( retValue.LastOrDefault()?.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNext ) ||
                      ( retValue.LastOrDefault()?.PlayAdjacentStrategy == ePlayAdjacentStrategy.PlayNextPrevious )) {
                    var followingTrack = sortedList.SkipWhile( t => !t.DbId.Equals( retValue.Last().DbId )).Skip( 1 ).FirstOrDefault();

                    if( followingTrack != null ) {
                        retValue.Add( followingTrack );
                    }
                    else {
                        break;
                    }
                }
            }

            return retValue;
        }
    }
}
