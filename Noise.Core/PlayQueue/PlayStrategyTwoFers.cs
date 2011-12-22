using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayQueue {
	internal class PlayStrategyTwoFers : IPlayStrategy {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;

		public PlayStrategyTwoFers( IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public PlayQueueTrack NextTrack( IPlayQueue queueMgr, IList<PlayQueueTrack> queue ) {
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

				var needA2Fer = true;
				if(( previousTrack != null ) &&
				   ( previousTrack.Artist != null ) &&
				   ( previousTrack.Artist.DbId == retValue.Artist.DbId )) {
					needA2Fer = false;
				}
				if(( nextTrack != null ) &&
				   ( nextTrack.Artist != null ) &&
				   ( nextTrack.Artist.DbId == retValue.Artist.DbId )) {
					needA2Fer = false;
				}

				if( needA2Fer ) {
					var albumList = mAlbumProvider.GetAlbumList( retValue.Artist );
					var r = new Random( DateTime.Now.Millisecond );
					var next = r.Next( albumList.List.Count() - 1 );
					var album = albumList.List.Skip( next ).FirstOrDefault();

					if( album != null ) {
						using( var trackList = mTrackProvider.GetTrackList( album )) {
							next = r.Next( trackList.List.Count() - 1 );
							var track = trackList.List.Skip( next ).FirstOrDefault();

							while(( track != null ) &&
							      ( queueMgr.IsTrackQueued( track ))) {
								next = r.Next( trackList.List.Count() - 1 );
								track = trackList.List.Skip( next ).FirstOrDefault();
							}

							if( track != null ) {
								queueMgr.StrategyAdd( track, retValue );
							}
						}
					}
				}
			}

			return( retValue );
		}
	}
}
