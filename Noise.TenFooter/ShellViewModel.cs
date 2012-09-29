using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.TenFoot.Ui.ViewModels;
using Noise.UI.ViewModels;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, IShell,
								  IHandle<Events.NavigateHome>, IHandle<Events.NavigateToScreen>, IHandle<Events.NavigateReturn> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IHome				mHomeView;
		private readonly TransportViewModel	mTransportViewModel;
		private readonly InputProcessor		mInputProcessor;
		private string						mScreenTitle;
		private string						mContextTitle;

		public ShellViewModel( IEventAggregator eventAggregator, InputProcessor inputProcessor,
							   IHome homeViewModel, TransportViewModel transportViewModel ) {
			mEventAggregator = eventAggregator;
			mHomeView = homeViewModel;
			mInputProcessor = inputProcessor;

			mTransportViewModel = transportViewModel;
			mTransportViewModel.IsActive = true;

			mEventAggregator.Subscribe( this );
		}

	    public PlayerViewModel PlayerView {
		    get{ return( mTransportViewModel ); }
	    }

	    public string ScreenTitle {
		    get{ return( mScreenTitle ); }
	    }

	    public string ContextTitle {
		    get{ return( mContextTitle ); }
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

		public void Handle( Events.NavigateHome data ) {
			NavigateHome();
		}

		public void Handle( Events.NavigateToScreen data ) {
			ActivateItem( data.ToScreen );
		}

		public void Handle( Events.NavigateReturn screenData ) {
			NavigateReturn( screenData.FromScreen, screenData.CloseScreen );
		}

    	public void NavigateHome() {
			while( ActiveItem != mHomeView ) {
				DeactivateItem( ActiveItem, true );
			}
    	}

		public bool CanNavigateBack {
			get{ return( ActiveItem != mHomeView ); }
		}

    	public void NavigateReturn( object fromScreen, bool closeScreen ) {
			DeactivateItem( fromScreen, closeScreen );

			if( ActiveItem == null ) {
				ActivateItem( mHomeView );
			}
    	}

		protected override void ChangeActiveItem( object newItem, bool closePrevious ) {
			base.ChangeActiveItem( newItem, closePrevious );

			if( newItem is ITitledScreen ) {
				var screen = newItem as ITitledScreen;

				mScreenTitle = screen.Title;
				mContextTitle = screen.Context;
			}
			else {
				mScreenTitle = string.Empty;
				mContextTitle = string.Empty;
			}

			NotifyOfPropertyChange( () => ScreenTitle );
			NotifyOfPropertyChange( () => ContextTitle );
			NotifyOfPropertyChange( () => CanNavigateBack );
		}
    }
}
