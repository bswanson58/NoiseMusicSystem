using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class TimeExplorerYearViewModel : AutomaticCommandBase {
		public	bool	YearValid { get; private set; }

		public TimeExplorerYearViewModel() {
			YearValid = false;
		}
	}
}
