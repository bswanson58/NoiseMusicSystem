using System.Windows;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters.DynamicProxies;

namespace Noise.UI.Support {
	public class DialogModelBase : ViewModelBase {
		private	object	mEditObject;
		private bool?	mDialogResult;

		public DialogModelBase() {
			mDialogResult = null;
		}

		public object EditObject {
			get{ return( mEditObject ); }
			set {
				mEditObject = value;

				RaisePropertyChanged( () => EditObject );
			}
		}

		public bool? DialogResult {
			get{ return( mDialogResult ); }
			set{ 
				mDialogResult = value; 

				RaisePropertyChanged( () => DialogResult );
			}
		}

		public void Execute_Ok( object sender ) {
			var proxy = mEditObject as DataErrorInfoProxy;

			if( proxy != null ) {
				proxy.Validate();

				if( proxy.HasErrors ) {
					MessageBox.Show( proxy.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
				else {
					DialogResult = true;
				}
			}
			else {
				DialogResult = true;
			}
		}

		public void Execute_Cancel( object sender ) {
			DialogResult = false;
		}
	}
}
