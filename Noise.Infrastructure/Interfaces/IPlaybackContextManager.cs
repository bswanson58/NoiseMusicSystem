using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlaybackContextManager {
		void			OpenContext( DbTrack forTrack );
		void			CloseContext( DbTrack forTrack );
	}
}
