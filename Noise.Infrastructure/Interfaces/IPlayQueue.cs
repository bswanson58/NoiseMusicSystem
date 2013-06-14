using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayStrategy {
		Next,
		Random,
		TwoFers
	}

	public enum ePlayExhaustedStrategy {
		Stop,
		Replay,
		PlayList,
		PlayArtist,
		PlaySimilar,
		PlayFavorites,
		PlayStream,
		PlayGenre,
		PlayCategory
	}

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
		PlayQueueTrack	PlayPreviousTrack();
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

		ePlayStrategy			PlayStrategy { get; }
		ePlayExhaustedStrategy	PlayExhaustedStrategy { get; }
		void					SetPlayStrategy( ePlayStrategy strategy, IPlayStrategyParameters parameters );
		void					SetPlayExhaustedStrategy( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters );

		void			StartPlayStrategy();
		bool			CanStartPlayStrategy { get; }

		IEnumerable<PlayQueueTrack>	PlayList { get; }
	}
}
