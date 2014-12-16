using Microsoft.Practices.Prism.Commands;

namespace Noise.Infrastructure {
	public static class GlobalCommands {
		public  static CompositeCommand SetFavorite = new CompositeCommand();
		public	static CompositeCommand	SetRating = new CompositeCommand();
		public	static CompositeCommand	SetMp3Tags = new CompositeCommand();

		public	static CompositeCommand	SetAlbumCover = new CompositeCommand();

		public	static CompositeCommand PlayTrack = new CompositeCommand();
		public	static CompositeCommand PlayTrackList = new CompositeCommand();
		public	static CompositeCommand	PlayAlbum = new CompositeCommand();
		public	static CompositeCommand PlayVolume = new CompositeCommand();
		public	static CompositeCommand	PlayStream = new CompositeCommand();

		public	static CompositeCommand	RequestLyrics = new CompositeCommand();

		public	static CompositeCommand	UpdatePlayCount = new CompositeCommand();

		public	static CompositeCommand	SynchronizeFromCloud = new CompositeCommand();

		public	static CompositeCommand ImportFavorites = new CompositeCommand();
		public	static CompositeCommand ImportRadioStreams = new CompositeCommand();
	}
}
