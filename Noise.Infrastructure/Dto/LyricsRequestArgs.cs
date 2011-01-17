namespace Noise.Infrastructure.Dto {
	public class LyricsRequestArgs {
		public	DbArtist	Artist { get; private set; }
		public	DbTrack		Track { get; private set; }

		public LyricsRequestArgs( DbArtist artist, DbTrack track ) {
			Artist = artist;
			Track = track;
		}
	}
}
