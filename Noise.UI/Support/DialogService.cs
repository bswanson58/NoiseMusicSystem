using System.Windows;
using Noise.UI.Adapters.DynamicProxies;

namespace Noise.UI.Support {
	public class DialogService : IDialogService {
		public bool? ShowDialog( Window dialog, object editObject ) {
			var proxy = new DataErrorInfoProxy( editObject );

			proxy.BeginEdit();
			proxy.Validate();

			if( dialog.DataContext is DialogModelBase ) {
				( dialog.DataContext as DialogModelBase ).EditObject = proxy;
			}

			var	dialogResult = dialog.ShowDialog();
			if( dialogResult.GetValueOrDefault( false )) {
				proxy.EndEdit();
			}
			else {
				proxy.CancelEdit();
			}

			return( dialogResult );
		}
	}
}
