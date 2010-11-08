using Microsoft.Practices.Composite.Presentation.Commands;

namespace Noise.Infrastructure {
	public static class GlobalCommands {
        public  static CompositeCommand SetFavorite = new CompositeCommand();
		public	static CompositeCommand	SetRating = new CompositeCommand();
	}
}
