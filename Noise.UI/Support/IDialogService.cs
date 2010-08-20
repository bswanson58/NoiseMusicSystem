using System.Windows;

namespace Noise.UI.Support {
	public interface IDialogService {
		bool?	ShowDialog( Window dialog, object editObject );
	}
}
