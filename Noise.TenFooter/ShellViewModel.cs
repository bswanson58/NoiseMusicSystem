using Caliburn.Micro;
using Noise.TenFoot.Ui.ViewModels;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, IShell {
		private HomeViewModel		mHomeView;

		protected override void OnActivate() {
			if( mHomeView == null ) {
				mHomeView = new HomeViewModel();
			}

			ActivateItem( mHomeView );
		}
    }
}
