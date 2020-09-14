using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using HueLighting;
using LightPipe;
using Prism.Ioc;
using MilkBottle.Interfaces;
using MilkBottle.Logging;
using MilkBottle.Properties;
using MilkBottle.Themes;
using MilkBottle.ViewModels;
using MilkBottle.Views;
using ReusableBits.Platform;
using Serilog.Events;

namespace MilkBottle {
   public partial class App : ISingleInstanceApp, IHandle<Events.LaunchRequest> {
        private IPlatformLog        mLog;
        private IEventAggregator    mEventAggregator;

        public App() {
            if(!SingleInstance<App>.InitializeAsFirstInstance( ApplicationConstants.ApplicationName )) {
                Shutdown();
            }

            DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

#if !DEBUG
//			var profileDirectory = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

//			System.Runtime.ProfileOptimization.SetProfileRoot( profileDirectory );
//          System.Runtime.ProfileOptimization.StartProfile( ApplicationConstants.ApplicationName );
#endif
        }

        public bool SignalExternalCommandLineArgs( IList<string> args ) {
            // Bring initial instance to foreground when a second instance is started.
            if( MainWindow?.DataContext is ShellViewModel vm ) {
                vm.ActivateShell();
            }

            return true;
        }

        protected override void OnExit( ExitEventArgs e ) {
            // Allow single instance code to perform cleanup operations
            SingleInstance<App>.Cleanup();
        }

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            new ApplicationModule().RegisterTypes( containerRegistry );
            new LightPipeModule().RegisterTypes( containerRegistry );
            new HueLightingModule().RegisterTypes( containerRegistry );

            // Caliburn Micro dispatcher initialize.
            PlatformProvider.Current = new XamlPlatformProvider();

            mLog = Container.Resolve<IPlatformLog>();
            mLog.AddLoggingSink( new MessageBoxSink(), LogEventLevel.Error );
            mLog.LogMessage( "+++ Application Started +++" );

            mEventAggregator = Container.Resolve<IEventAggregator>();
            mEventAggregator.Subscribe( this );
        }

        protected override Window CreateShell() {
            var module = new ApplicationModule();
            module.OnInitialized( Container );

            var themeCatalog = Container.Resolve<ThemeCatalog>();
            ThemeManager.SetApplicationTheme( String.Empty, themeCatalog.Signatures.FirstOrDefault( t => t.Name.Equals( "Orange" ))?.Location );

            var retValue = Container.Resolve<Shell>();
            retValue.Closing += OnShellClose;

            return retValue;
        }

        private void OnShellClose( object sender, CancelEventArgs args ) {
            mEventAggregator.PublishOnUIThread( new Events.ApplicationClosing());
            mEventAggregator.Unsubscribe( this );

            mLog.LogMessage( "+++ Application Closing +++" );
        }

        private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e ) {
            mLog.LogException( "Application Domain unhandled exception", e.ExceptionObject as Exception );

            Shutdown( -1 );
        }

        private void AppDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e ) {
            if( Debugger.IsAttached ) {
                Clipboard.SetText( e.Exception.ToString());
            }

            mLog.LogException( "Application Dispatcher unhandled exception", e.Exception );

            e.Handled = true;
            Shutdown( -1 );
        }

        private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e ) {
            mLog.LogException( "Task Scheduler unobserved exception", e.Exception );

            e.SetObserved(); 
        }
        
        public void Handle( Events.LaunchRequest eventArgs ) {
            try {
                Process.Start( eventArgs.Target );
            }
            catch( Exception ex ) {
                mLog.LogException( "OnLaunchRequest:", ex );
            }
        }
    }
}