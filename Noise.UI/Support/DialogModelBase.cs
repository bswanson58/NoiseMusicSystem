using Noise.Infrastructure.Support;

namespace Noise.UI.Support {
	public class DialogModelBase : ViewModelBase {
		public dynamic          EditObject { get; set; }
        public IDialogWindow    DialogWindow { get; set; }
	}
}
