using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Threading;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Properties;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;
using Application = System.Windows.Application;

namespace MilkBottle.ViewModels {
    class ShellViewModel : PropertyChangeBase {
        private const int                       cHeartbeatSeconds = 30;

        private readonly IStateManager          mStateManager;
        private readonly IEventAggregator       mEventAggregator;
        private readonly IDialogService         mDialogService;
        private readonly IPreferences           mPreferences;
        private readonly IPlatformLog           mLog;
        private readonly IIpcHandler            mIpcHandler;
        private readonly DispatcherTimer        mIpcTimer;
        private readonly JavaScriptSerializer   mSerializer;
        private readonly string                 mIpcIcon;
        private NotifyIcon			            mNotifyIcon;
        private Window                          mShell;
        private WindowState					    mStoredWindowState;

        public  DelegateCommand                 Configuration { get; }

        public  ObservableCollection<UiCompanionApp>    CompanionApplications { get; }
        public  bool                            HaveCompanionApplications => CompanionApplications.Any();

        public  bool                            DisplayStatus { get; private set; }
        public  bool                            DisplayController { get; private set; }

        public ShellViewModel( IStateManager stateManager, IPreferences preferences, IDialogService dialogService, IIpcHandler ipcHandler,
                               IEventAggregator eventAggregator, IPlatformLog log ) {
            mStateManager = stateManager;
            mPreferences = preferences;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;
            mLog = log;
            mIpcHandler = ipcHandler;

            Configuration = new DelegateCommand( OnConfiguration );
            CompanionApplications = new ObservableCollection<UiCompanionApp>();

            mNotifyIcon = new NotifyIcon { BalloonTipTitle = ApplicationConstants.ApplicationName, Text = ApplicationConstants.ApplicationName }; 
            mNotifyIcon.Click += OnNotifyIconClick;

            var iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/Milk Bottle.ico" ));
            if( iconStream != null ) {
                mNotifyIcon.Icon = new System.Drawing.Icon( iconStream.Stream );
            }

            mIpcHandler.Initialize( ApplicationConstants.ApplicationName, ApplicationConstants.EcosystemName, OnIpcMessage );
            mSerializer = new JavaScriptSerializer();

            mIpcTimer = new DispatcherTimer( DispatcherPriority.Background ) { Interval = TimeSpan.FromSeconds( cHeartbeatSeconds )};
            mIpcTimer.Tick += OnIpcTimer;
            mIpcTimer.Start();

            iconStream = Application.GetResourceStream( new Uri( "pack://application:,,,/MilkBottle;component/Resources/ApplicationIcon.xaml" ));
            if( iconStream != null ) {
                var reader = new StreamReader( iconStream.Stream );

                mIpcIcon = reader.ReadToEnd();
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
            mIpcTimer.Stop();

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

        private void OnIpcMessage( BaseIpcMessage message ) {
            Execute.OnUIThread( () => {
                switch( message.Subject ) {
                    case NoiseIpcSubject.cCompanionApplication:
                        AddCompanionApp( message );
                        break;

                    case NoiseIpcSubject.cActivateApplication:
                        ActivateShell();
                        break;
                }
            });
        }

        private void AddCompanionApp( BaseIpcMessage message ) {
            try {
                var companionApp = mSerializer.Deserialize<CompanionApplication>( message.Body );

                if( companionApp != null ) {
                    var existingApp = CompanionApplications.FirstOrDefault( a => a.ApplicationName.Equals( companionApp.Name ));

                    if( existingApp == null ) {
                        using( var stream = new MemoryStream( Encoding.UTF8.GetBytes( companionApp.Icon ))) {
                            var icon = XamlReader.Load( stream ) as FrameworkElement;

                            CompanionApplications.Add( new UiCompanionApp( companionApp.Name, icon, $"Switch to {companionApp.Name}", OnCompanionAppRequest ));

                            RaisePropertyChanged( () => HaveCompanionApplications );
                        }
                    }
                    else {
                        existingApp.UpdateHeartbeat();
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellViewModel.AddCompanionApp", ex );
            }
        }

        private void OnCompanionAppRequest( UiCompanionApp app ) {
            try {
                var message = new ActivateApplication();
                var json = mSerializer.Serialize( message );

                mIpcHandler.SendMessage( app.ApplicationName, NoiseIpcSubject.cActivateApplication, json );

                mShell.WindowState = WindowState.Minimized;
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellVIewModel.OnCompanionAppRequest", ex );
            }
        }

        private void OnIpcTimer( object sender, EventArgs args ) {
            try {
                var message = new CompanionApplication( ApplicationConstants.ApplicationName, mIpcIcon );
                var json = mSerializer.Serialize( message );

                mIpcHandler.BroadcastMessage( NoiseIpcSubject.cCompanionApplication, json );

                var appList = new List<UiCompanionApp>( CompanionApplications );

                appList.ForEach( a => {
                    var expiration = DateTime.Now - TimeSpan.FromSeconds( cHeartbeatSeconds * 2 );

                    if( a.LastHeartbeat < expiration ) {
                        CompanionApplications.Remove( a );
                    }
                });

                RaisePropertyChanged( () => HaveCompanionApplications );
            }
            catch( Exception ex ) {
                mLog.LogException( "ShellViewModel.OnIpcTimer", ex );
            }
        }
    } 
}
