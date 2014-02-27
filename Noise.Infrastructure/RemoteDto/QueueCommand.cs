namespace Noise.Infrastructure.RemoteDto {
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
