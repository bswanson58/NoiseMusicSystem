using System;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
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
                                   IHandle<Events.ExternalPlayerSwitch> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IPreferences			mPreferences;
		private SmallPlayerView					mPlayerView;
		private Window							mShell;
		private NotifyIcon						mNotifyIcon;
		private WindowState						mStoredWindowState;

		public WindowManager( IEventAggregator eventAggregator, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mPreferences = preferences;

			mStoredWindowState = WindowState.Normal;
			mNotifyIcon = new NotifyIcon { BalloonTipTitle = Constants.ApplicationName, Text = Constants.ApplicationName }; 
			mNotifyIcon.Click += OnNotifyIconClick;

            mEventAggregator.Subscribe( this );
		}

		public void Initialize( Window shell ) {
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

        public void ToggleSmallPlayer() {
            if( mPlayerView == null ) {
                ShowSmallPlayer();
            }
            else {
                CloseSmallPlayer();
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

			mNotifyIcon?.Dispose();
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
	}
}
