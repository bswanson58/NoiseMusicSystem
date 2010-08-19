namespace Noise.Infrastructure.Dto {
	public class StreamInfo {
		public int		Channel { get; private set; }
		public string	Artist { get; private set; }
		public string	Album { get; private set; }
		public string	Title { get; private set; }
		public string	Genre { get; private set; }

		public StreamInfo( int channel, string artist, string album, string title, string genre ) {
			Channel = channel;
			Artist = artist;
			Album = album;
			Title = title;
			Genre = genre;
		}
	}
}
