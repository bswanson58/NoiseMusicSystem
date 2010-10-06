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

		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		PlayQueueTrack	PlayingTrack { get; }
		int				PlayingTrackReplayCount { get; set; }
		bool			IsQueueEmpty { get; }
		int				UnplayedTrackCount { get; }
		bool			StrategyRequestsQueued { get; }

		ePlayStrategy				PlayStrategy { get; set; }
		ePlayExhaustedStrategy		PlayExhaustedStrategy { get; set; }

		IEnumerable<PlayQueueTrack>	PlayList { get; }
	}
}
