using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using Application = System.Windows.Application;

namespace MilkBottle.ViewModels {
    public enum ShellView {
        Manual,
        Review,
        Sync,
        Browse
    }

    class ShellViewModel : PropertyChangeBase, IHandle<Events.CloseLightPipeController> {
        private readonly IStateManager          mStateManager;
        private readonly IIpcManager            mIpcManager;
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IPreferences           mPreferences;
        private NotifyIcon			            mNotifyIcon;
        private Window                          mShell;
        private WindowState					    mStoredWindowState;
        private IDisposable                     mCompanionAppsSubscription;
        private IDisposable                     mActivationSubscription;

        public  ShellView                       ShellViewDisplayed { get; private set; }
        public  DelegateCommand                 Configuration { get; }
        public  DelegateCommand                 LightPipe { get; }
        public  DelegateCommand                 DisplayManualController { get; }
        public  DelegateCommand                 DisplayReviewer { get; }
        public  DelegateCommand                 DisplaySyncView { get; }
        public  DelegateCommand                 DisplayBrowseView { get; }

        public  ObservableCollection<UiCompanionApp>    CompanionApplications { get; }
        public  bool                            HaveCompanionApplications => CompanionApplications.Any();

        public  bool                            DisplayStatus { get; private set; }
        public  bool                            DisplayController { get; private set; }
        public  bool                            DisplayLightPipeController { get; private set; }

        public ShellViewModel( IStateManager stateManager, IPreferences preferences, IDialogService dialogService, IIpcManager ipcManager,
                               IEventAggregator eventAggregator, IApplicationConstants applicationConstants ) {
            mStateManager = stateManager;
            mIpcManager = ipcManager;
            mPreferences = preferences;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;

            Configuration = new DelegateCommand( OnConfiguration );
            LightPipe = new DelegateCommand( OnLightPipe );
            DisplayManualController = new DelegateCommand( OnDisplayManualController );
            DisplayReviewer = new DelegateCommand( OnDisplayReviewer );
            DisplaySyncView = new DelegateCommand( OnDisplaySyncView );
            DisplayBrowseView = new DelegateCommand( OnDisplayBrowseView );
            CompanionApplications = new ObservableCollection<UiCompanionApp>();

            mNotifyIcon = new NotifyIcon { BalloonTipTitle = applicationConstants.ApplicationName, Text = applicationConstants.ApplicationName }; 
            mNotifyIcon.Click += OnNotifyIconClick;

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/Milk Bottle.ico" ));
            if( iconStream != null ) {
                mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
            }

            DisplayStatus = true;
            DisplayController = true;
            ShellViewDisplayed = ShellView.Manual;

            DisplayLightPipeController = false;

            mCompanionAppsSubscription = mIpcManager.CompanionAppsUpdate.Subscribe( OnCompanionAppsUpdate );
            mActivationSubscription = mIpcManager.OnActivationRequest.Subscribe( OnActivationRequest );

            mEventAggregator.Subscribe( this );
        }

        private void OnCompanionAppsUpdate( IEnumerable<ActiveCompanionApp> apps ) {
            CompanionApplications.Clear();
            CompanionApplications.AddRange( from app in apps select new UiCompanionApp( app, $"Switch to {app.ApplicationName}", OnCompanionAppRequest ));
    
            RaisePropertyChanged( () => HaveCompanionApplications );
        }

        private void OnActivationRequest( bool state ) {
            if( state ) {
                ActivateShell();
            }
        }

        private void OnCompanionAppRequest( UiCompanionApp app ) {
            mIpcManager.ActivateApplication( app.ApplicationName );

            mShell.WindowState = WindowState.Minimized;
        }

        public void ShellLoaded( Window shell ) {
            mShell = shell;

            mShell.StateChanged += OnShellStateChanged;
            mShell.IsVisibleChanged += OnShellVisibleChanged;
            mShell.Closing += OnShellClosing;

            SwitchView( ShellView.Manual );
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
            mCompanionAppsSubscription?.Dispose();
            mCompanionAppsSubscription = null;

            mActivationSubscription?.Dispose();
            mActivationSubscription = null;

            mIpcManager.Dispose();

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
            mDialogService.ShowDialog( nameof( ConfigurationDialog ), null, OnConfigurationCompleted );
        }

        private void OnConfigurationCompleted( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                mEventAggregator.PublishOnUIThread( new Events.MilkConfigurationUpdated());
            }
        }

        public void OnLightPipe() {
            DisplayLightPipeController = true;

            RaisePropertyChanged( () => DisplayLightPipeController );
        }

        public void Handle( Events.CloseLightPipeController args ) {
            DisplayLightPipeController = false;

            RaisePropertyChanged( () => DisplayLightPipeController );
        }

        private void OnDisplayManualController() {
            SwitchView( ShellView.Manual );
        }

        private void OnDisplayReviewer() {
            SwitchView( ShellView.Review );
        }

        private void OnDisplaySyncView() {
            SwitchView( ShellView.Sync );
        }

        private void OnDisplayBrowseView() {
            SwitchView( ShellView.Browse );
        }

        private void SwitchView( ShellView toView ) {
            if( toView != ShellViewDisplayed ) {
                mStateManager.EnterState( eStateTriggers.Stop );
                ShellViewDisplayed = toView;

                RaisePropertyChanged( () => ShellViewDisplayed );
                mEventAggregator.PublishOnUIThread( new Events.ModeChanged( toView ));
            }
        }
    } 
}
