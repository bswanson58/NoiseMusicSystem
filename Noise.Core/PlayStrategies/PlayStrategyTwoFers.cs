using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	public class PlayStrategyTwoFers : PlayStrategyBase {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;

		public PlayStrategyTwoFers( IAlbumProvider albumProvider, ITrackProvider trackProvider ) :
			base( ePlayStrategy.TwoFers, "Two-Fers", "Plays a randomly chosen second track from the same artist for each track played." ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

	    protected override string FormatDescription() {
            return( "with two-fers from each artist" );
	    }

		public override PlayQueueTrack NextTrack() {
            var queue = new List<PlayQueueTrack>( PlayQueueMgr.PlayList );
			var retValue = queue.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed ));

			if(( retValue != null ) &&
			   ( retValue.Artist != null )) {
				var targetIndex = queue.IndexOf( retValue );
				PlayQueueTrack	previousTrack = null;
				PlayQueueTrack	nextTrack = null;

				if( targetIndex > 0 ) {
					previousTrack = queue[targetIndex - 1];
				}

				if(( targetIndex + 1 ) < queue.Count ) {
					nextTrack = queue[targetIndex + 1];
				}

			    var needA2Fer = !(( previousTrack != null ) &&
			                      ( previousTrack.Artist != null ) &&
			                      ( previousTrack.Artist.DbId == retValue.Artist.DbId ));
			    if(( nextTrack != null ) &&
				   ( nextTrack.Artist != null ) &&
				   ( nextTrack.Artist.DbId == retValue.Artist.DbId )) {
					needA2Fer = false;
				}

				if( needA2Fer ) {
					var r = new Random( DateTime.Now.Millisecond );

					using( var albumList = mAlbumProvider.GetAlbumList( retValue.Artist )) {
						var next = r.Next( albumList.List.Count() - 1 );
						var album = albumList.List.Skip( next ).FirstOrDefault();

						if( album != null ) {
							using( var trackList = mTrackProvider.GetTrackList( album )) {
								var goodList = ( from t in trackList.List where t.Rating >= 0 select t ).ToList();

								next = r.Next( goodList.Count() - 1 );
								var track = goodList.Skip( next ).FirstOrDefault();

								while(( track != null ) &&
									  ( PlayQueueMgr.IsTrackQueued( track ))) {
									next = r.Next( goodList.Count() - 1 );
									track = goodList.Skip( next ).FirstOrDefault();
								}

								if( track != null ) {
									PlayQueueMgr.StrategyAdd( track, retValue );
								}
							}
						}
					}
				}
			}

			return( retValue );
		}
	}
}
