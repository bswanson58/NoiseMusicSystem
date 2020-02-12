using Caliburn.Micro;
using MilkBottle.Interfaces;
using MilkBottle.Logging;
using MilkBottle.Models;
using MilkBottle.Platform;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;

namespace MilkBottle {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IAudioManager, AudioManager>();
            containerRegistry.RegisterSingleton<IMilkController, MilkController>();

            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
