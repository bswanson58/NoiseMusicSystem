using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Support {
	public class DialogModelBase : AutomaticCommandBase {
		public dynamic          EditObject { get; set; }
        public IDialogWindow    DialogWindow { get; set; }
	}
}
