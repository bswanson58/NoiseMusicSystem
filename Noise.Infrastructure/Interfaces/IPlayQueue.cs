using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayStrategy {
		Next,
		Random
	}

	public enum ePlayExhaustedStrategy {
		Stop,
		Replay,
		PlayList,
		PlaySimilar,
		PlayFavorites,
		PlayStream
	}

	public interface IPlayQueue {
		void			Add( DbTrack track );
		void			Add( DbAlbum album );
		void			Add( DbArtist artist );
		void			Add( DbInternetStream stream );
		void			StrategyAdd( DbTrack track );
		void			ClearQueue();

		PlayQueueTrack	PlayNextTrack();
		PlayQueueTrack	PlayPreviousTrack();
		void			StopPlay();
		void			ReplayQueue();

		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		PlayQueueTrack	PlayingTrack { get; set; }
		int				PlayingTrackReplayCount { get; set; }
		bool			IsQueueEmpty { get; }
		bool			IsTrackQueued( DbTrack track );
		int				UnplayedTrackCount { get; }
		bool			StrategyRequestsQueued { get; }

		void			ReorderQueueItem( int fromIndex, int toIndex );

		ePlayStrategy			PlayStrategy { get; set; }
		ePlayExhaustedStrategy	PlayExhaustedStrategy { get; }
		void					SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, long itemId );

		IEnumerable<PlayQueueTrack>	PlayList { get; }
	}
}
