using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Noise.AppSupport;
using Noise.Desktop.Properties;
using Noise.Desktop.ViewModels;
using Noise.Desktop.Views;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Models;
using Noise.UI.Support;
using Noise.UI.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;
using ReusableBits.Platform;

namespace Noise.Desktop {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : ISingleInstanceApp {
        private INoiseManager		mNoiseManager;
        private INoiseWindowManager	mWindowManager;
        private Window				mShell;
        private	ApplicationSupport	mAppSupport;
        private IApplicationLog		mLog;

		public App() {
            if( SingleInstance<App>.InitializeAsFirstInstance( Constants.ApplicationName )) {
                DispatcherUnhandledException += AppDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException +=CurrentDomainUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;
            }
            else {
                Shutdown();
            }
        }

        public bool SignalExternalCommandLineArgs( IList<string> args ) {
            // Bring initial instance to foreground when a second instance is started.
            mWindowManager?.ActivateShell();

            return true;
        }

        protected override void OnStartup( StartupEventArgs e ) {
            base.OnStartup( e );
#if !DEBUG
			var profileDirectory = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

			System.Runtime.ProfileOptimization.SetProfileRoot( profileDirectory );
			System.Runtime.ProfileOptimization.StartProfile( "Noise Desktop Startup.Profile" );
#endif
		}

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            // Caliburn Micro dispatcher initialize.
            PlatformProvider.Current = new XamlPlatformProvider();
    
            RegisterNoiseModules();

            var iocConfig = Container.Resolve<IocConfiguration>();
            iocConfig.InitializeIoc( ApplicationUsage.Desktop );

            mLog = Container.Resolve<IApplicationLog>();
            mLog.ApplicationStarting();

            mWindowManager = Container.Resolve<INoiseWindowManager>();

#if( DEBUG )
            BindingErrorListener.Listen( message => MessageBox.Show( message ));
#endif
        }

        protected override Window CreateShell() {
            mShell = Container.Resolve<ShellView>();
            mShell.Closing += OnShellClosing;
            mShell.Show();

            mWindowManager.Initialize( mShell );

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupView ));

            var preferences = Container.Resolve<UserInterfacePreferences>();
            ThemeManager.SetApplicationTheme( preferences.ThemeName, preferences.ThemeSignature );

            StartNoise();

            return ( mShell );
        }

        private void OnShellClosing( object sender, System.ComponentModel.CancelEventArgs e ) {
            if(!mNoiseManager.CanShutDown( out var shutdownReason )) {
                var dialogService = Container.Resolve<IDialogService>();

                dialogService.ShowDialog( nameof( ExitApplicationDialogView ), new DialogParameters{{ ExitApplicationDialogViewModel.cReasonParameter, shutdownReason } }, result => {
                    if( result.Result == ButtonResult.OK ) {
                        StopNoise();
                    }
                    else {
                        e.Cancel = true;
                    }
                });
            }
            else {
                StopNoise();
            }
        }

        private async void StartNoise() {
            mNoiseManager = Container.Resolve<INoiseManager>();
            mAppSupport = Container.Resolve<ApplicationSupport>();

            mAppSupport.Initialize();

            if( await mNoiseManager.AsyncInitialize()) {
                OnCoreInitialized();
            }
        }

        private void OnCoreInitialized() {
            var preferences = Container.Resolve<NoiseCorePreferences>();
            var eventAggregator = Container.Resolve<IEventAggregator>();
            var libraryConfiguration = Container.Resolve<ILibraryConfiguration>();

            InitializeNoiseModules();

            if(( preferences?.LoadLastLibraryOnStartup == true ) &&
               ( preferences.LastLibraryUsed != Constants.cDatabaseNullOid )) {
                libraryConfiguration.Open( preferences.LastLibraryUsed );

                eventAggregator.BeginPublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
            }
            else {
                eventAggregator.BeginPublishOnUIThread( new Events.WindowLayoutRequest( libraryConfiguration.Libraries.Any() ? Constants.LibrarySelectionLayout : Constants.LibraryCreationLayout ));
            }
        }

        public void StopNoise() {
            mAppSupport.Shutdown();

            mNoiseManager?.Shutdown();
            mNoiseManager = null;

            mWindowManager?.Shutdown();
            mWindowManager = null;

            Settings.Default.Save();

            mLog.ApplicationExiting();
        }

        protected override void OnExit( ExitEventArgs e ) {
            // Allow single instance code to perform cleanup operations
            SingleInstance<App>.Cleanup();
        }

        private void RegisterNoiseModules() {
            RegisterModule( typeof( DesktopModule ));
            RegisterModule( typeof( Core.NoiseCoreModule ));
            RegisterModule( typeof( AudioSupport.AudioSupportModule ));
            RegisterModule( typeof( BlobStorage.BlobStorageModule ));
            RegisterModule( typeof( Metadata.NoiseMetadataModule ));
            RegisterModule( typeof( RemoteHost.RemoteHostModule ));
            RegisterModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));
            RegisterModule( typeof( UI.NoiseUiModule ));
            RegisterModule( typeof( Guide.GuideModule ));
        }

        private void RegisterModule( Type moduleType ) {
            if( Container.Resolve( moduleType ) is IModule module ) {
                module.RegisterTypes( Container as IContainerRegistry);
            }
        }

        private void InitializeNoiseModules() {
            InitializeModule( typeof( DesktopModule ));
            InitializeModule( typeof( Core.NoiseCoreModule ));
            InitializeModule( typeof( AudioSupport.AudioSupportModule ));
            InitializeModule( typeof( BlobStorage.BlobStorageModule ));
            InitializeModule( typeof( Metadata.NoiseMetadataModule ));
            InitializeModule( typeof( RemoteHost.RemoteHostModule ));
            InitializeModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ));
            InitializeModule( typeof( UI.NoiseUiModule ));
            InitializeModule( typeof( Guide.GuideModule ));
        }

        private void InitializeModule( Type moduleType ) {
            if( Container.Resolve( moduleType ) is IModule module ) {
                module.OnInitialized( Container );
            }
        }

        private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
		    mLog?.LogException( "Application Domain unhandled exception", e.ExceptionObject as Exception );

		    Shutdown( -1 );
		}

		private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
			if( Debugger.IsAttached ) {
				Clipboard.SetText( e.Exception.ToString());
			}

		    mLog?.LogException( "Application Dispatcher unhandled exception", e.Exception );

		    e.Handled = true;
			Shutdown( -1 );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) {
		    mLog?.LogException( "Task Scheduler unobserved exception", e.Exception );

		    e.SetObserved(); 
    	}
    }
}
