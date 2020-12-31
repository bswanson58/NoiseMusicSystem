using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Platform;
using Noise.RemoteClient.Views;
using Prism;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;

namespace Noise.RemoteClient {
    public partial class App {
        private IClientManager  mClientManager;
        private IPlatformLog    mLog;

        public App( IPlatformInitializer initializer )
            : base( initializer ) {
        }

        protected override void OnInitialized() {
//            Device.SetFlags( new [] { "AppTheme_Experimental" });
//            Current.UserAppTheme = OSAppTheme.Dark;

            InitializeComponent();

            mClientManager = Container.Resolve<IClientManager>();
            mLog = Container.Resolve<IPlatformLog>();

            ThemeManager.LoadInitialTheme();

            MainPage = new AppShell();
        }

        protected override void CleanUp() {
            mClientManager.StopClientManager();
            mLog.LogMessage( "Application Cleanup" );

            base.CleanUp();
        }

        protected override void OnStart() {
            base.OnStart();

            mLog.LogMessage( "Application Starting" );
            mClientManager?.OnApplicationStarting();
        }

        protected override void OnSleep() {
            mClientManager?.OnApplicationStopping();
            mLog.LogMessage( "Application Sleeping" );

            base.OnSleep();
        }

        protected override void OnResume() {
            base.OnResume();

            mLog.LogMessage( "Application Resuming" );
            mClientManager?.OnApplicationStarting();
        }

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            RemoteClientModule.RegisterServices( containerRegistry );
            RemoteClientModule.RegisterNavigation( containerRegistry );
        }
    }
}
