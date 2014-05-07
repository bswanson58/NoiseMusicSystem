using System.Windows;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Noise.Librarian.Views;
using Noise.UI.Support;

namespace Noise.Librarian {
	public class Bootstrapper : UnityBootstrapper {
		private Window				mShell;

		protected override DependencyObject CreateShell() {
			mShell = Container.Resolve<Shell>();
			mShell.Show();
			mShell.Closing += OnShellClosing;

#if( DEBUG )
			BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif

			return ( mShell );
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			Shutdown();
		}

		private void Shutdown() {
			
		}
	}
}
