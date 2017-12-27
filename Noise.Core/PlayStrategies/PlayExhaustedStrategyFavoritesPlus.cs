using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Core.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
    class PlayExhaustedStrategyFavoritesPlus : PlayExhaustedListBase {
        private readonly ITrackProvider     mTrackProvider;

        public PlayExhaustedStrategyFavoritesPlus(ITrackProvider trackProvider, ILogPlayStrategy log) :
            base( ePlayExhaustedStrategy.PlayFavoritesPlus, "Play Favorites Plus", "Play favorites plus rated tracks.", log ) {
            mTrackProvider = trackProvider;
        }

        protected override string FormatDescription() {
            return ("play favorite tracks");
        }

        protected override void FillTrackList(long itemId) {
            mTrackList.Clear();

            try {
                using( var list = mTrackProvider.GetFavoriteTracks()) {
                    foreach( var track in list.List ) {
                        if( !PlayQueueMgr.IsTrackQueued( track )) {
                            mTrackList.Add( track );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                Log.LogException( "Queuing tracks", ex );
            }
        }

        protected override bool QueueTracks( int count ) {
            var retValue= false;

            for( int addedCount = 0; addedCount < count; addedCount++ ) {
                if( mTrackList.Count > 0 ) {
                    var selectedFavorite = SelectTrack();
                    var associatedTracks = new List<DbTrack>();

                    if( selectedFavorite != null ) {
                        var albumTracks = mTrackProvider.GetTrackList( selectedFavorite.Album );
                        var ratedTracks = albumTracks.List.Where( t => t.Rating >= 4 ).ToList();

                        if( ratedTracks.Any()) {
                            var r = new Random( DateTime.Now.Millisecond );
                            var tracksToAdd = r.Next( ratedTracks.Count );

                            for( var c = 0; c < tracksToAdd; c++ ) {
                                var next = r.Next( ratedTracks.Count - 1 );
                                var track = ratedTracks.Skip( next ).FirstOrDefault();

                                if( track != null ) {
                                    associatedTracks.Add( track );
                                    ratedTracks.Remove( track );
                                }
                            }
                        }
                    }

                    PlayQueueMgr.StrategyAdd( selectedFavorite );

                    foreach( var track in associatedTracks ) {
                        PlayQueueMgr.StrategyAdd( track );
                        addedCount++;
                    }


                    retValue = true;
                }
            }

            return retValue;
        }
    }
}
