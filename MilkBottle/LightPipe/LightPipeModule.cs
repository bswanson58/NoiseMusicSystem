using LightPipe.Interfaces;
using LightPipe.Models;
using Prism.Ioc;
using Prism.Modularity;

namespace LightPipe {
    public class LightPipeModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IImageProcessor, ImageProcessor>();
            containerRegistry.RegisterSingleton<IZoneManager, ZoneManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
