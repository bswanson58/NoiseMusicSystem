namespace Noise.Infrastructure.Dto {
	public enum CommandRequestCategory {
		Queue,
		Transport,
		View,
		Unknown
	}

	public enum CommandRequestItem {
		FinishPlaying,
		NextTrack,
		PausePlaying,
		StartPlaying,
		StopPlaying,
		PreviousTrack,
		ReplayTrack,
		Unknown
	}

	public class CommandRequest {
		public CommandRequestCategory	Category { get; }
		public CommandRequestItem		Command { get; }

		public CommandRequest( CommandRequestCategory category, CommandRequestItem command ) {
			Category = category;
			Command = command;
		}
	}
}
