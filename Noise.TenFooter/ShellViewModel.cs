using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, INavigate, IShell {
		private readonly IHome				mHomeView;
		private readonly PlayerViewModel	mPlayerViewModel;
		private readonly InputProcessor		mInputProcessor;

		public ShellViewModel( IHome homeViewModel, PlayerViewModel playerViewModel, InputProcessor inputProcessor ) {
			mHomeView = homeViewModel;
			mInputProcessor = inputProcessor;

			mPlayerViewModel = playerViewModel;
			mPlayerViewModel.IsActive = true;
		}

	    public PlayerViewModel PlayerView {
		    get{ return( mPlayerViewModel ); }
	    }

		protected override void OnActivate() {
			ActivateItem( mHomeView );
		}

		protected override void OnViewAttached( object view, object context ) {
			base.OnViewAttached( view, context );

			if( view is Window ) {
				var helper = new WindowInteropHelper( view as Window );

				mInputProcessor.Initialize( helper.Handle );
			}
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
