using Caliburn.Micro;
using MilkBottle.Interfaces;
using MilkBottle.Logging;
using MilkBottle.Models;
using MilkBottle.Models.PresetTimers;
using MilkBottle.Platform;
using MilkBottle.ViewModels;
using MilkBottle.Views;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;
using ReusableBits.Platform;

namespace MilkBottle {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IAudioManager, AudioManager>();
            containerRegistry.RegisterSingleton<IInitializationController, InitializationController>();
            containerRegistry.RegisterSingleton<IMilkController, MilkController>();
            containerRegistry.RegisterSingleton<ProjectMWrapper, ProjectMWrapper>();
            containerRegistry.RegisterSingleton<IPresetController, PresetController>();
            containerRegistry.RegisterSingleton<IPresetLibrarian, PresetLibrarian>();
            containerRegistry.RegisterSingleton<IStateManager, StateManager>();
            containerRegistry.RegisterSingleton<IPresetTimerFactory, PresetTimerFactory>();

            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();

            containerRegistry.RegisterDialog<SelectPresetDialog, SelectPresetDialogModel>();
            containerRegistry.RegisterDialog<ConfigurationDialog, ConfigurationDialogModel>();

            containerRegistry.RegisterSingleton<IIpcHandler, IpcHandler>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
