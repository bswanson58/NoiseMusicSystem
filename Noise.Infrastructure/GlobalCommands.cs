using Microsoft.Practices.Prism.Commands;

namespace Noise.Infrastructure {
	public static class GlobalCommands {
		public	static CompositeCommand	SetMp3Tags = new CompositeCommand();

		public	static CompositeCommand	RequestLyrics = new CompositeCommand();

		public	static CompositeCommand	UpdatePlayCount = new CompositeCommand();

		public	static CompositeCommand	SynchronizeFromCloud = new CompositeCommand();

		public	static CompositeCommand ImportFavorites = new CompositeCommand();
		public	static CompositeCommand ImportRadioStreams = new CompositeCommand();
        public  static CompositeCommand ImportUserTags = new CompositeCommand();
	}
}
