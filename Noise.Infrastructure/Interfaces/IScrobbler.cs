using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IScrobbler {
		void	TrackStarted( PlayQueueTrack track );
		void	TrackPlayed( PlayQueueTrack track );
	}
}
