namespace Noise.Infrastructure.Dto {
	public class PlayQueueTrack {
		public DbTrack		Track { get; private set; }
		public StorageFile	File { get; private set; }
		public bool			IsPlaying { get; set; }
		public bool			HasPlayed { get; set; }

		public PlayQueueTrack( DbTrack track, StorageFile file ) {
			Track = track;
			File = file;
		}
	}
}
