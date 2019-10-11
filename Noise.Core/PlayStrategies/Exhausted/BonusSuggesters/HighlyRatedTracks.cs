using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies.Exhausted.BonusSuggesters {
    class HighlyRatedTracks : ExhaustedHandlerBase {
        private const string                cBreadCrumb = "Highly Rated Tracks";
        private readonly ITrackProvider     mTrackProvider;

        public HighlyRatedTracks( ITrackProvider trackProvider ) :
            base( eTrackPlayHandlers.HighlyRatedTracks, eTrackPlayStrategy.BonusSuggester, "Highly Rated Tracks", "Add some highly rated tracks from the same album." ) {
            mTrackProvider = trackProvider;
        }

        public override void SelectTrack( IExhaustedSelectionContext context ) {
            context.SelectedTracks.ToList().ForEach( 
                track => {
                    if( context.BreadCrumbs.ContainsKey( track.Album )) {
                        var crumbs = context.BreadCrumbs[track.Album];

                        if(!crumbs.Contains( cBreadCrumb )) {
                            crumbs.Add( cBreadCrumb );

                            SelectOtherTracks( track, context );
                        }
                    }
                    else {
                        context.BreadCrumbs.Add( track.Album, new List<string>{ cBreadCrumb });

                        SelectOtherTracks( track, context );
                    }
                });
        }

        private void SelectOtherTracks( DbTrack forTrack, IExhaustedSelectionContext context ) {
            using( var albumTracks = mTrackProvider.GetTrackList( forTrack.Album )) {
                if( albumTracks.List.Any()) {
                    var ratedTracks = albumTracks.List.Where( t => t.Rating >= 4 ).ToList();

                    if( ratedTracks.Any()) {
                        var tracksToAdd = Math.Min( NextRandom( ratedTracks.Count ), 5 );

                        for( var c = 0; c < tracksToAdd; c++ ) {
                            var track = SelectRandomTrack( ratedTracks );

                            if( CanSuggestTrack( track, context )) {
                                context.SelectedTracks.Insert( context.SelectedTracks.IndexOf( forTrack ) + 1, track );
                                ratedTracks.Remove( track );
                            }
                        }
                    }
                }
            }
        }
    }
}
