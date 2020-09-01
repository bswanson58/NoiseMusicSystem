using LightPipe.Interfaces;
using LightPipe.Models;
using LightPipe.ViewModels;
using LightPipe.Views;
using MilkBottle.Infrastructure.Interfaces;
using Prism.Ioc;
using Prism.Modularity;

namespace LightPipe {
    public class LightPipeModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IImageProcessor, ImageProcessor>();
            containerRegistry.RegisterSingleton<IZoneManager, ZoneManager>();

            containerRegistry.RegisterDialog<ZoneEditView, ZoneEditViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
