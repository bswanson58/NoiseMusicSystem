﻿using System;
using System.Windows;
using System.Windows.Forms;
using Composite.Layout;
using Composite.Layout.Configuration;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Desktop.Properties;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.UI.Views;
using Application = System.Windows.Application;

namespace Noise.Desktop {
	internal class WindowManager {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly ILayoutManager		mLayoutManager;
		private SmallPlayerView				mPlayerView;
		private Window						mShell;
		private NotifyIcon					mNotifyIcon;
		private WindowState					mStoredWindowState;

		public WindowManager( IUnityContainer container ) {
			mContainer = container;
			mLayoutManager = LayoutConfigurationManager.LayoutManager;

			mEvents = mContainer.Resolve<IEventAggregator>();
			mEvents.GetEvent<Events.WindowLayoutRequest>().Subscribe( OnWindowLayoutRequested );
			mEvents.GetEvent<Events.ExternalPlayerSwitch>().Subscribe( OnExternalPlayerSwitch );

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

		public void OnWindowLayoutRequested( string forLayout ) {
			switch( forLayout ) {
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
					   (!string.Equals( mLayoutManager.CurrentLayout.Name, forLayout )) &&
					   ( mLayoutManager.Layouts.Exists( layout => string.Equals( layout.Name, forLayout )))) {
						mLayoutManager.LoadLayout( forLayout );
					}
					break;
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

		private bool ShouldMinimizeToTray() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			
			return( configuration.MinimizeToTray );
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
	}
}
