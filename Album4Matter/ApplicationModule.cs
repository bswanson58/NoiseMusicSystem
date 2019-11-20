using Album4Matter.Interfaces;
using Album4Matter.Logging;
using Album4Matter.Platform;
using Album4Matter.ViewModels;
using Caliburn.Micro;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;

namespace Album4Matter {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();

            containerRegistry.RegisterSingleton<IEnvironment, OperatingEnvironment>();
            containerRegistry.RegisterSingleton<IFileWriter, JsonObjectWriter>();
            containerRegistry.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
            containerRegistry.RegisterSingleton<IPreferences, PreferencesManager>();
            containerRegistry.RegisterSingleton<IPlatformDialogService, PlatformDialogService>();

            containerRegistry.Register<IItemInspectionViewModel, ItemInspectionViewModel>();
            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
