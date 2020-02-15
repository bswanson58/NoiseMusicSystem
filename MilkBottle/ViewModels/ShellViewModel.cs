using System;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;
using Application = System.Windows.Application;

namespace MilkBottle.ViewModels {
    public class ShellViewModel : PropertyChangeBase {
        private readonly IEventAggregator   mEventAggregator;
        private readonly IEnvironment       mEnvironment;
        private readonly IPreferences       mPreferences;
        private readonly NotifyIcon			mNotifyIcon;
        private Window                      mShell;
        private WindowState                 mStoredWindowState;
        private WindowState                 mCurrentWindowState;

        public  bool        DisplayStatus { get; private set; }

        public ShellViewModel( IEnvironment environment, IPreferences preferences, IEventAggregator eventAggregator ) {
            mEnvironment = environment;
            mPreferences = preferences;
            mEventAggregator = eventAggregator;

            mNotifyIcon = new NotifyIcon { BalloonTipTitle = mEnvironment.ApplicationName(), Text = mEnvironment.ApplicationName() }; 
            mNotifyIcon.Click += OnNotifyIconClick;

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/Milk Bottle.ico" ));
            if( iconStream != null ) {
                mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
            }

            mStoredWindowState = WindowState.Normal;
            DisplayStatus = true;
        }

        public void ShellLoaded( Window shell ) {
            mShell = shell;
        }

        private void OnNotifyIconClick( object sender, EventArgs args ) {
            if( mShell != null ) {
                mShell.Show();
                mShell.WindowState = mStoredWindowState;
                mShell.Activate();
            }
        }

        public WindowState CurrentWindowState {
            get => mCurrentWindowState;
            set {
                mCurrentWindowState = value;

                DisplayStatus = mCurrentWindowState != WindowState.Maximized;
                RaisePropertyChanged( () => DisplayStatus );

                if(( mCurrentWindowState == WindowState.Minimized ) &&
                   ( ShouldMinimizeToTray())) {
                    mShell?.Hide();
                    mNotifyIcon.Visible = true;
                }
                else {
                    mStoredWindowState = mShell.WindowState;
                    mNotifyIcon.Visible = false;
                }

                mEventAggregator.PublishOnUIThread( new Events.WindowStateChanged( mCurrentWindowState ));
            }
        }

        private bool ShouldMinimizeToTray() {
            var retValue = false;
            var configuration = mPreferences.Load<MilkPreferences>();
			
            if( configuration != null ) {
                retValue = configuration.ShouldMinimizeToTray;
            }

            return( retValue );
        }

    } 
}
