using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlaybackContextWriter {
		ScPlayContext	GetAlbumContext( DbTrack forTrack );
		ScPlayContext	GetTrackContext( DbTrack forTrack );

		void			SaveAlbumContext( DbTrack forTrack, ScPlayContext albumContext );
		void			SaveTrackContext( DbTrack forTrack, ScPlayContext trackContext );
	}
}
