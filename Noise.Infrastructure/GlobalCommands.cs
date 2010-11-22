using Microsoft.Practices.Prism.Commands;

namespace Noise.Infrastructure {
	public static class GlobalCommands {
        public  static CompositeCommand SetFavorite = new CompositeCommand();
		public	static CompositeCommand	SetRating = new CompositeCommand();

		public	static CompositeCommand	UpdatePlayCount = new CompositeCommand();
	}
}
