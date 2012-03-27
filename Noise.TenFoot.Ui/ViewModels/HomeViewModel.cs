using Caliburn.Micro;

namespace Noise.TenFoot.Ui.ViewModels {
	public class HomeViewModel : Screen {
		public BindableCollection<string>	MenuChoices{ get; private set; }

		public HomeViewModel() {
			MenuChoices = new BindableCollection<string> { "Library", "Favorites", "Queue", "What's New" };
		}
	}
}
