﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Composite.Layout;
using Composite.Layout.Configuration;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Noise.Desktop.Properties;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Views;
using ReusableBits.Ui.Utility;
using Application = System.Windows.Application;

namespace Noise.Desktop.Models {
	internal class WindowManager : INoiseWindowManager,
                                   IHandle<Events.WindowLayoutRequest>, IHandle<Events.ViewDisplayRequest>,
								   IHandle<Events.ExternalPlayerSwitch>, IHandle<Events.ExtendedPlayerRequest>, IHandle<Events.StandardPlayerRequest> {
		private readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEventAggregator;
		private readonly IPreferences			mPreferences;
		private readonly ILayoutManager			mLayoutManager;
		private readonly IRegionManager			mRegionManager;
		private SmallPlayerView					mPlayerView;
		private Window							mShell;
		private NotifyIcon						mNotifyIcon;
		private WindowState						mStoredWindowState;

		public WindowManager( IUnityContainer container, IEventAggregator eventAggregator, IPreferences preferences ) {
			mContainer = container;
			mEventAggregator = eventAggregator;
			mPreferences = preferences;

			mLayoutManager = LayoutConfigurationManager.LayoutManager;
			mRegionManager = mContainer.Resolve<IRegionManager>();

			mStoredWindowState = WindowState.Normal;
			mNotifyIcon = new NotifyIcon { BalloonTipTitle = Constants.ApplicationName, Text = Constants.ApplicationName }; 
			mNotifyIcon.Click += OnNotifyIconClick;

            mEventAggregator.Subscribe( this );
		}

		public void Initialize( Window shell ) {
			mLayoutManager.Initialize( mContainer );

			mShell = shell;
			mShell.StateChanged += OnShellStateChanged;
			mShell.IsVisibleChanged += OnShellVisibleChanged;
			mShell.Closing += OnShellClosing;

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/Noise.Desktop;component/Noise.ico" ));
            if( iconStream != null ) {
                mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
            }

			ExecutingEnvironment.ResizeWindowIntoVisibility( mShell );
		}

		public void Shutdown() {
			CloseSmallPlayer();
		}

		public void Handle( Events.WindowLayoutRequest eventArgs ) {
			ChangeWindowLayout( eventArgs.LayoutName );
		}

		public void ChangeWindowLayout( string toLayout ) {
            switch( toLayout ) {
                case Constants.SmallPlayerViewToggle:
                    if( mPlayerView == null ) {
                        ShowSmallPlayer();
                    }
                    else {
                        CloseSmallPlayer();
                    }
                    break;

                default:
                    var currentLayoutName = mLayoutManager.CurrentLayout != null ? mLayoutManager.CurrentLayout.Name : string.Empty;

                    if((!string.Equals( currentLayoutName, toLayout )) &&
                       ( mLayoutManager.Layouts.Exists( layout => string.Equals( layout.Name, toLayout )))) {
                        mLayoutManager.LoadLayout( toLayout );

                        GC.Collect();
                    }
                    break;
            }
        }

		public void Handle( Events.ViewDisplayRequest eventArgs ) {
			IRegion region;

			switch( eventArgs.ViewName ) {
				case ViewNames.AlbumInfoView:
				case ViewNames.ArtistInfoView:
				case ViewNames.ArtistTracksView:
                case ViewNames.RatedTracksView:
                    region = mRegionManager.Regions.FirstOrDefault( r => r.Name == "AlbumInfo" );

					if( region != null ) {
                        Execute.OnUIThread( () => region.RequestNavigate( eventArgs.ViewName ));
                    }
					break;

				case ViewNames.RelatedTracksView:
                    region = mRegionManager.Regions.FirstOrDefault( r => r.Name == "LeftPanel" );

                    if( region != null ) {
                        Execute.OnUIThread( () => region.RequestNavigate( eventArgs.ViewName ));
                    }
					break;
			}
		}

		private void ShowSmallPlayer() {
			if( mPlayerView == null ) {
				mPlayerView = new SmallPlayerView { Top = Settings.Default.SmallPlayerTop, Left = Settings.Default.SmallPlayerLeft };

				ExecutingEnvironment.MoveWindowIntoVisibility( mPlayerView );

			    var interfacePreferences = mPreferences.Load<UserInterfacePreferences>();

			    ThemeManager.SetApplicationResources( mPlayerView, new Uri( interfacePreferences.ThemeSignature ));
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

		private bool ShouldMinimizeToTray() {
			var retValue = false;
			var configuration = mPreferences.Load<UserInterfacePreferences>();
			
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
			mEventAggregator.Unsubscribe( this );

			mNotifyIcon.Dispose();
			mNotifyIcon = null;
		}

		private void OnNotifyIconClick( object sender, EventArgs e ) {
			ActivateShell();
		}

		public void Handle( Events.ExternalPlayerSwitch eventArgs ) {
			ActivateShell();
		}

		public void ActivateShell() {
			if( mShell != null ) {
				mShell.Show();
				mShell.WindowState = mStoredWindowState;
				mShell.Activate();
			}
		}

		public void DeactivateShell() {
            mShell.WindowState = WindowState.Minimized;
        }

		public void Handle( Events.StandardPlayerRequest eventArgs ) {
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

		public void Handle( Events.ExtendedPlayerRequest eventArgs ) {
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