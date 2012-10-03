namespace Noise.TenFoot.Ui.Interfaces {
	public enum eMainMenuCommand {
		Favorites,
		Library,
		Queue
	}

	public interface IHomeScreen : ITitledScreen {
		string				MenuTitle { get; }
		string				Description { get; }
		eMainMenuCommand	MenuCommand { get; }
		int					ScreenOrder { get; }
	}
}
