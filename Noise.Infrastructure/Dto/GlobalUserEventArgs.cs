namespace Noise.Infrastructure.Dto {
	public enum UserEventAction {
		StopPlay,
		PausePlay,
		StartPlay,
		PlayNextTrack,
		PlayPreviousTrack,
		ReplayTrack,
		None
	}

	public class GlobalUserEventArgs {
		public	UserEventAction		EventAction { get; private set; }

		public GlobalUserEventArgs( UserEventAction action ) {
			EventAction = action;
		}
	}
}
