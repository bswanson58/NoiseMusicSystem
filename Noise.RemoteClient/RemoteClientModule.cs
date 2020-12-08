using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Models;
using Noise.RemoteClient.Services;
using Noise.RemoteClient.ViewModels;
using Noise.RemoteClient.Views;
using Prism.Ioc;
using Xamarin.Forms;

namespace Noise.RemoteClient {
    static class RemoteClientModule {
        public static void RegisterServices( IContainerRegistry container ) {
            container.RegisterSingleton<IClientManager, ClientManager>();
            container.RegisterSingleton<IServiceLocator, ServiceLocator>();
        }

        public static void RegisterNavigation( IContainerRegistry container ) {
            container.RegisterForNavigation<NavigationPage>();
            container.RegisterForNavigation<MainPage, MainPageViewModel>();
        }
    }
}
