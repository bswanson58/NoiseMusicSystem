using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Prism.Ioc;
using TuneArchiver.Interfaces;
using TuneArchiver.Views;

namespace TuneArchiver {
    public partial class App {
        private IPlatformLog    mLog;

        protected override void OnStartup( StartupEventArgs e ) {
            // Caliburn Micro dispatcher initialize.
            PlatformProvider.Current = new XamlPlatformProvider();

            base.OnStartup( e );

            mLog = Container.Resolve<IPlatformLog>();
            mLog.LogMessage( "+++ Application Started +++" );

            DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException +=CurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

#if !DEBUG
//			var profileDirectory = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName );

//			System.Runtime.ProfileOptimization.SetProfileRoot( profileDirectory );
//          System.Runtime.ProfileOptimization.StartProfile( ApplicationConstants.ApplicationName );
#endif
        }

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            var module = new ApplicationModule();

            module.RegisterTypes( containerRegistry );
        }

        protected override Window CreateShell() {
            var retValue = Container.Resolve<Shell>();

            retValue.Closing += OnShellClose;

            return retValue;
        }

        private void OnShellClose( object sender, CancelEventArgs args ) {
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
    }
}