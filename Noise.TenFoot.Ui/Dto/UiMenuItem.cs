using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Noise.TenFoot.Ui.Dto {
	public enum eMainMenuCommand {
		Favorites,
		Library,
		Search,
		Queue
	}

	[DebuggerDisplay( "Menu Item: {Title}")] 
	public class UiMenuItem {
		public string			Title { get; private set; }
		public BitmapImage		Image { get; private set; }
		public eMainMenuCommand	Command { get; private set; }

		public UiMenuItem( eMainMenuCommand command, string title, BitmapImage image ) {
			Command = command;
			Title = title;
			Image = image;
		}

		public override string ToString() {
			return( Title );
		}
	}
}
