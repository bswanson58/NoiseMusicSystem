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
		AddSimilar
	}

	public interface IPlayQueue {
		void			Add( DbTrack track );
		void			Add( DbAlbum album );
		void			Add( DbArtist artist );
		void			ClearQueue();

		PlayQueueTrack	PlayNextTrack();
		PlayQueueTrack	PlayPreviousTrack();
		void			StopPlay();

		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		PlayQueueTrack	PlayingTrack { get; }
		bool			IsQueueEmpty { get; }

		ePlayStrategy				PlayStrategy { get; set; }
		ePlayExhaustedStrategy		PlayExhaustedStrategy { get; set; }

		IEnumerable<PlayQueueTrack>	PlayList { get; }
	}
}
