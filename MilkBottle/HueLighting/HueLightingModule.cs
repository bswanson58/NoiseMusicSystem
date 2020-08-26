using HueLighting.Interfaces;
using HueLighting.Models;
using Prism.Ioc;
using Prism.Modularity;

namespace HueLighting {
    public class HueLightingModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IHubManager, HubManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
