using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayQueue {
		void			Add( DbTrack track );
		void			Add( DbAlbum album );
		void			Add( DbArtist artist );
		void			Add( DbInternetStream stream );
		void			StrategyAdd( DbTrack track );
		void			StrategyAdd( DbTrack track, PlayQueueTrack afterTrack );
		void			RemoveTrack( PlayQueueTrack track );
		void			RemovePlayedTracks();
		void			ClearQueue();

		PlayQueueTrack	PlayNextTrack();
        bool            CanPlayNextTrack();
		PlayQueueTrack	PlayPreviousTrack();
        bool            CanPlayPreviousTrack();
		void			StopPlay();
		void			ReplayQueue();
		void			ContinuePlayFromTrack( PlayQueueTrack track );

		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		PlayQueueTrack	PlayingTrack { get; set; }
		int				PlayingTrackReplayCount { get; set; }
		bool			IsQueueEmpty { get; }
		bool			IsTrackQueued( DbTrack track );
		int				UnplayedTrackCount { get; }
		int				PlayedTrackCount { get; }
		bool			StrategyRequestsQueued { get; }

		void			ReorderQueueItem( int fromIndex, int toIndex );

		IPlayStrategy			PlayStrategy { get; }
		IPlayExhaustedStrategy	PlayExhaustedStrategy { get; }
		void					SetPlayStrategy( ePlayStrategy strategyId, IPlayStrategyParameters parameters );
		void					SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters );

		void			StartPlayStrategy();
		bool			CanStartPlayStrategy { get; }

		IEnumerable<PlayQueueTrack>	PlayList { get; }
	}
}
