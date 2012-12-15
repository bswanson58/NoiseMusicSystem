using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Caliburn.Micro;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using Noise.TenFoot.Ui.ViewModels;
using Noise.UI.ViewModels;

namespace Noise.TenFooter {
    public class ShellViewModel : Conductor<object>.Collection.OneActive, IShell,
								  IHandle<Events.NavigateHome>, IHandle<Events.NavigateToScreen>, IHandle<Events.NavigateReturn> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IWindowManager			mWindowManager;
		private readonly IHome					mHomeView;
		private readonly ConfigurationViewModel	mConfiguration;
		private readonly TransportViewModel		mTransportViewModel;
		private readonly InputProcessor			mInputProcessor;
		private readonly DispatcherTimer		mTimer;
		private DateTime						mCurrentTime;
		private string							mScreenTitle;
		private string							mContextTitle;
		private bool							mDialogActive;

		public ShellViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration,
							   InputProcessor inputProcessor, IWindowManager windowManager,
							   IHome homeViewModel, TransportViewModel transportViewModel, ConfigurationViewModel configurationViewModel ) {
			mEventAggregator = eventAggregator;
			mWindowManager = windowManager;
			mHomeView = homeViewModel;
			mConfiguration = configurationViewModel;
			mInputProcessor = inputProcessor;

			mTransportViewModel = transportViewModel;
			mTransportViewModel.IsActive = true;

			CurrentTime = DateTime.Now;
			mTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 3.0D ) };
			mTimer.Tick += OnTimer;
			mTimer.Start();

			mEventAggregator.Subscribe( this );

			libraryConfiguration.OpenDefaultLibrary();
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

	    public DateTime CurrentTime {
		    get{ return( mCurrentTime ); }
			set {
				if( mCurrentTime != value ) {
					mCurrentTime = value;

					NotifyOfPropertyChange( () => CurrentTime );
				}
			}
	    }

		private void OnTimer( object sender, EventArgs args ) {
			CurrentTime = DateTime.Now;
		}

		protected override void OnActivate() {
			DisplayName = "Noise At 10feet";

			ActivateItem( mHomeView );
		}

		protected override void OnViewAttached( object view, object context ) {
			base.OnViewAttached( view, context );

			if( view is Window ) {
				var helper = new WindowInteropHelper( view as Window );

				mInputProcessor.Initialize( helper.Handle, ProcessInput );
			}
		}

		private void ProcessInput( InputEvent input ) {
			if(!mDialogActive ) {
				switch( input.Command ) {
					case InputCommand.Configuration:
						mDialogActive = true;
						mWindowManager.ShowDialog( mConfiguration );
						mDialogActive = false;
						break;

					default:
						mHomeView.ProcessInput( input );
						break;
				}
			}
			
		}

		public void Handle( Events.NavigateHome data ) {
			NavigateHome();
		}

		public void Handle( Events.NavigateToScreen data ) {
			if( data.ToScreen is IHomeScreen ) {
				var screen = data.ToScreen as IHomeScreen;

				if( screen.MenuCommand == eMainMenuCommand.Exit ) {
					TryClose();

					return;
				}
			}

			ActivateItem( data.ToScreen );
		}

		public void Handle( Events.NavigateReturn screenData ) {
			NavigateReturn( screenData.FromScreen, screenData.CloseScreen );
		}

    	public void NavigateHome() {
			while( ActiveItem != mHomeView ) {
				if( ActiveItem == null ) {
					ActivateItem( mHomeView );
				}
				else {
					DeactivateItem( ActiveItem, true );
				}
			}
    	}

		public void NavigateBack() {
			NavigateReturn( ActiveItem, true );
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

				mScreenTitle = screen.ScreenTitle;
				if(!string.IsNullOrWhiteSpace( screen.Context )) {
					mContextTitle = screen.Context + ":";
				}
				else {
					mContextTitle = string.Empty;
				}
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
