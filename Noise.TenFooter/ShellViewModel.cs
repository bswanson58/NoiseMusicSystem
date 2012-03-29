using Caliburn.Micro;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, INavigate, IShell {
		private readonly IHome		mHomeView;

		public ShellViewModel( IHome homeViewModel ) {
			mHomeView = homeViewModel;
		}

		protected override void OnActivate() {
			ActivateItem( mHomeView );
		}

    	public void NavigateHome() {
			while( ActiveItem != mHomeView ) {
				DeactivateItem( ActiveItem, true );
			}
    	}

    	public void NavigateTo( object screen ) {
			ActivateItem( screen );
    	}

    	public void NavigateReturn( object fromScreen, bool closeScreen ) {
			DeactivateItem( fromScreen, closeScreen );
    	}
    }
}
