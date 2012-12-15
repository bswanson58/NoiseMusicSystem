using Noise.TenFoot.Ui.Interfaces;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ExitViewModel : IHomeScreen {
		public string ScreenTitle { get; private set; }
		public string Context { get; private set; }
		public string MenuTitle { get; private set; }
		public string Description { get; private set; }
		public eMainMenuCommand MenuCommand { get; private set; }
		public int ScreenOrder { get; private set; }

		public ExitViewModel() {
			ScreenTitle = "Exit";
			MenuTitle = "Exit";

			ScreenOrder = 99;
			MenuCommand = eMainMenuCommand.Exit;

			Description = "Close the Noise Music System";
			Context = string.Empty;
		}
	}
}
