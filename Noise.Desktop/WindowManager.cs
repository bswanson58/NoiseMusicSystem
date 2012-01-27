using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Composite.Layout;
using Composite.Layout.Configuration;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Noise.Desktop.Properties;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.UI.Views;
using Application = System.Windows.Application;

namespace Noise.Desktop {
	internal class WindowManager : IHandle<Events.WindowLayoutRequest> {
		private readonly IUnityContainer	mContainer;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IEventAggregator	mEvents;
		private readonly ILayoutManager		mLayoutManager;
		private readonly IRegionManager		mRegionManager;
		private SmallPlayerView				mPlayerView;
		private Window						mShell;
		private NotifyIcon					mNotifyIcon;
		private WindowState					mStoredWindowState;

		public WindowManager( IUnityContainer container, ICaliburnEventAggregator eventAggregator ) {
			mContainer = container;
			mEventAggregator = eventAggregator;

			mLayoutManager = LayoutConfigurationManager.LayoutManager;
			mRegionManager = mContainer.Resolve<IRegionManager>();

			mEventAggregator.Subscribe( this );

			mEvents = mContainer.Resolve<IEventAggregator>();
			mEvents.GetEvent<Events.ExternalPlayerSwitch>().Subscribe( OnExternalPlayerSwitch );
			mEvents.GetEvent<Events.StandardPlayerRequest>().Subscribe( OnStandardPlayerRequest );
			mEvents.GetEvent<Events.ExtendedPlayerRequest>().Subscribe( OnExtendedPlayerRequest );
			mEvents.GetEvent<Events.NavigationRequest>().Subscribe( OnNavigationRequest );

			mStoredWindowState = WindowState.Normal;
			mNotifyIcon = new NotifyIcon { //BalloonTipText = "Click the tray icon to show.", 
										   BalloonTipTitle = Constants.ApplicationName, 
										   Text = Constants.ApplicationName }; 
			mNotifyIcon.Click += OnNotifyIconClick;

			var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/Noise.Desktop;component/Noise.ico" ));
			if( iconStream != null ) {
				mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
			}
		}

		public void Initialize( Window shell ) {
			mLayoutManager.Initialize( mContainer );
			mLayoutManager.LoadLayout( Constants.ExploreLayout );

			mShell = shell;
			mShell.StateChanged += OnShellStateChanged;
			mShell.IsVisibleChanged += OnShellVisibleChanged;
			mShell.Closing += OnShellClosing;
		}

		public void Shutdown() {
			CloseSmallPlayer();
		}

		public void Handle( Events.WindowLayoutRequest eventArgs ) {
			switch( eventArgs.LayoutName ) {
				case Constants.SmallPlayerViewToggle:
					if( mPlayerView == null ) {
						ShowSmallPlayer();
					}
					else {
						CloseSmallPlayer();
					}
					break;

				default:
					if(( mLayoutManager.CurrentLayout != null ) &&
					   (!string.Equals( mLayoutManager.CurrentLayout.Name, eventArgs.LayoutName )) &&
					   ( mLayoutManager.Layouts.Exists( layout => string.Equals( layout.Name, eventArgs.LayoutName )))) {
						mLayoutManager.LoadLayout( eventArgs.LayoutName );
					}
					break;
			}
		}

		private void OnNavigationRequest( NavigationRequestArgs args ) {
			var region = mRegionManager.Regions.FirstOrDefault( r => r.Name == "AlbumInfo" );

			if( region != null ) {
				switch( args.RequestingViewName ) {
					case ViewNames.AlbumView:
						region.RequestNavigate( ViewNames.ArtistTracksView );
						break;

					case ViewNames.ArtistTracksView:
						region.RequestNavigate( ViewNames.AlbumView );
						break;
				}
			}
		}

		private void ShowSmallPlayer() {
			if( mPlayerView == null ) {
				mPlayerView = new SmallPlayerView { Top = Settings.Default.SmallPlayerTop, Left = Settings.Default.SmallPlayerLeft };
			}
			mPlayerView.Show();
		}

		private void CloseSmallPlayer() {
			if( mPlayerView != null ) {
				Settings.Default.SmallPlayerTop = mPlayerView.Top;
				Settings.Default.SmallPlayerLeft = mPlayerView.Left;

				mPlayerView.Close();
				mPlayerView = null;
			}
		}

		private static bool ShouldMinimizeToTray() {
			var retValue = false;
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			
			if( configuration != null ) {
				retValue = configuration.MinimizeToTray;
			}

			return( retValue );
		}

		private void OnShellStateChanged( object sender, EventArgs e ) {
			if(( ShouldMinimizeToTray()) &&
			   ( mShell != null )) {
				if( mShell.WindowState == WindowState.Minimized ) {
					mShell.Hide();
//					if( mNotifyIcon != null ) {
//						mNotifyIcon.ShowBalloonTip( 2000 );
//					}

					ShowSmallPlayer();
				}
				else {
					mStoredWindowState = mShell.WindowState;

					CloseSmallPlayer();
				}
			}
		}

		private void OnShellVisibleChanged( object sender, DependencyPropertyChangedEventArgs e ) {
			if(( mNotifyIcon != null ) &&
			   ( mShell != null )) {
				if( ShouldMinimizeToTray()) {
					mNotifyIcon.Visible = !mShell.IsVisible;
				}
				else {
					mNotifyIcon.Visible = false;
				}
			}
		}

		private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
			mNotifyIcon.Dispose();
			mNotifyIcon = null;
		}

		private void OnNotifyIconClick( object sender, EventArgs e ) {
			ActivateShell();
		}

		private void OnExternalPlayerSwitch( object sender ) {
			ActivateShell();
		}

		private void ActivateShell() {
			if( mShell != null) {
				mShell.Show();
				mShell.WindowState = mStoredWindowState;
				mShell.Activate();
			}
		}

		private void OnStandardPlayerRequest( object sender ) {
			var	region = mRegionManager.Regions["Player"];

			if( region != null ) {
				System.Windows.Controls.UserControl playerView = region.Views.OfType<PlayerView>().Select( view => view as System.Windows.Controls.UserControl ).FirstOrDefault();

				if( playerView == null ) {
					playerView = new PlayerView();

					mRegionManager.AddToRegion( "Player", playerView );
				}

				region.Activate( playerView );
			}
		}

		private void OnExtendedPlayerRequest( object sender ) {
			var	region = mRegionManager.Regions["Player"];

			if( region != null ) {
				System.Windows.Controls.UserControl extendedView = region.Views.OfType<PlayerExtendedView>().Select( view => view as System.Windows.Controls.UserControl ).FirstOrDefault();

				if( extendedView == null ) {
					extendedView = new PlayerExtendedView();

					mRegionManager.AddToRegion( "Player", extendedView );
				}

				region.Activate( extendedView );
			}
		}
	}
}
