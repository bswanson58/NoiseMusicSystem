using Noise.Desktop.Models;
using Noise.Desktop.ViewModels;
using Noise.Desktop.Views;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using ReusableBits.Platform;

namespace Noise.Desktop {
    public class DesktopModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<INoiseWindowManager, WindowManager>();

            containerRegistry.RegisterSingleton<IIpcManager, IpcManager>();
            containerRegistry.RegisterSingleton<IIpcHandler, IpcHandler>();
            containerRegistry.Register<IPlaybackPublisher, PlaybackPublisher>();

            containerRegistry.RegisterDialog<ExitApplicationDialogView, ExitApplicationDialogViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
			var regionManager = containerProvider.Resolve<IRegionManager>();

//            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupLibraryCreationView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( StartupLibrarySelectionView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( LibraryView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( ListeningView ));
            regionManager.RegisterViewWithRegion( RegionNames.ShellView, typeof( TimelineView ));
        }
    }
}
