using System;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Properties;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using Application = System.Windows.Application;

namespace MilkBottle.ViewModels {
    class ShellViewModel : PropertyChangeBase {
        private readonly IStateManager      mStateManager;
        private readonly IEventAggregator   mEventAggregator;
        private readonly IDialogService     mDialogService;
        private readonly IPreferences       mPreferences;
        private NotifyIcon			        mNotifyIcon;
        private Window                      mShell;
        private WindowState					mStoredWindowState;

        public  DelegateCommand             Configuration { get; }

        public  bool                        DisplayStatus { get; private set; }
        public  bool                        DisplayController { get; private set; }

        public ShellViewModel( IStateManager stateManager, IPreferences preferences, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mStateManager = stateManager;
            mPreferences = preferences;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;

            Configuration = new DelegateCommand( OnConfiguration );

            mNotifyIcon = new NotifyIcon { BalloonTipTitle = ApplicationConstants.ApplicationName, Text = ApplicationConstants.ApplicationName }; 
            mNotifyIcon.Click += OnNotifyIconClick;

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/Milk Bottle.ico" ));
            if( iconStream != null ) {
                mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
            }

            DisplayStatus = true;
            DisplayController = true;
        }

        public void ShellLoaded( Window shell ) {
            mShell = shell;

            mShell.StateChanged += OnShellStateChanged;
            mShell.IsVisibleChanged += OnShellVisibleChanged;
            mShell.Closing += OnShellClosing;
        }

        private void OnShellStateChanged( object sender, EventArgs args ) {
            if( mShell != null ) {
                if( ShouldMinimizeToTray()) {
                    if( mShell.WindowState == WindowState.Minimized ) {
                        mShell.Hide();
                    }
                    else {
                        mStoredWindowState = mShell.WindowState;
                    }
                }

                DisplayStatus = mShell.WindowState != WindowState.Maximized;
                RaisePropertyChanged( () => DisplayStatus );

                DisplayController = mShell.WindowState != WindowState.Maximized || ShouldDisplayController();
                RaisePropertyChanged( () => DisplayController );

                mStateManager.EnterState( mShell.WindowState == WindowState.Minimized ? eStateTriggers.Suspend : eStateTriggers.Resume );

                mEventAggregator.PublishOnUIThread( new Events.WindowStateChanged( mShell.WindowState ));
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

            if( mShell != null ) {
                mShell.StateChanged -= OnShellStateChanged;
                mShell.IsVisibleChanged -= OnShellVisibleChanged;
                mShell.Closing -= OnShellClosing;
            }
        }

        private void OnNotifyIconClick( object sender, EventArgs e ) {
            ActivateShell();
        }

        public void ActivateShell() {
            if( mShell != null ) {
                mShell.Show();
                mShell.WindowState = mStoredWindowState;
                mShell.Activate();
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

        private bool ShouldDisplayController() {
            var retValue = false;
            var configuration = mPreferences.Load<MilkPreferences>();
			
            if( configuration != null ) {
                retValue = configuration.DisplayControllerWhenMaximized;
            }

            return( retValue );
        }

        public void OnConfiguration() {
            mDialogService.ShowDialog( "ConfigurationDialog", null, OnConfigurationCompleted );
        }

        private void OnConfigurationCompleted( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                mEventAggregator.PublishOnUIThread( new Events.MilkConfigurationUpdated());
            }
        }
    } 
}
