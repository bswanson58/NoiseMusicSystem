using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Views;
using Prism;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;

namespace Noise.RemoteClient {
    public partial class App {
        private IClientManager  mClientManager;

        public App( IPlatformInitializer initializer )
            : base( initializer ) {
        }

        protected override void OnInitialized() {
            InitializeComponent();

            mClientManager = Container.Resolve<IClientManager>();
            mClientManager.StartClientManager();

            MainPage = new AppShell();
        }

        protected override void CleanUp() {
            mClientManager.StopClientManager();

            base.CleanUp();
        }

        protected override void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            RemoteClientModule.RegisterServices( containerRegistry );
            RemoteClientModule.RegisterNavigation( containerRegistry );
        }
    }
}
