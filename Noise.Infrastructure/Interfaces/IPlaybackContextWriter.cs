using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlaybackContextWriter {
		PlaybackContext	GetAlbumContext( DbTrack forTrack );
		PlaybackContext	GetTrackContext( DbTrack forTrack );

		void			SaveAlbumContext( DbTrack forTrack, ScPlayContext albumContext );
		void			SaveTrackContext( DbTrack forTrack, ScPlayContext trackContext );

		ScTrackPlayPoints	GetTrackPlayPoints( DbTrack forTrack );
		void				SaveTrackPlayPoints( DbTrack forTrack, ScTrackPlayPoints fadePoints );
	}
}
