using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.RemoteDto {
	// Mirrors ePlayState in IPlayController
	public enum PlayState {
		Stopped = 1,
		Playing = 2,
		Paused = 3
	}

	public class PlayStateUtility {
		public static int ConvertPlayState( ePlayState playState ) {
			var retValue = 0;

			switch( playState ) {
				case ePlayState.Stopped:
				case ePlayState.StoppedEmptyQueue:
				case ePlayState.Stopping:
					retValue = (int)PlayState.Stopped;
					break;

				case ePlayState.Playing:
				case ePlayState.PlayNext:
				case ePlayState.PlayPrevious:
				case ePlayState.Resuming:
				case ePlayState.ExternalPlay:
					retValue = (int)PlayState.Playing;
					break;

				case ePlayState.Paused:
				case ePlayState.Pausing:
					retValue = (int)PlayState.Paused;
					break;
			}

			return( retValue );
		}
	}

	public enum QueueCommand {
		StartPlaying = 1,
		Clear = 2,
		ClearPlayed = 3
	}

	public enum QueueItemCommand {
		Remove = 1,
		PlayNext = 2,
		ReplayTrack = 3
	}
}
