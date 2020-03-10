using Caliburn.Micro;
using MilkBottle.Database;
using MilkBottle.Interfaces;
using MilkBottle.Logging;
using MilkBottle.Models;
using MilkBottle.Models.PresetSequencers;
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
            containerRegistry.RegisterSingleton<IDatabaseBuilder, DatabaseBuilder>();
            containerRegistry.RegisterSingleton<IDatabaseProvider, DatabaseProvider>();
            containerRegistry.RegisterSingleton<IInitializationController, InitializationController>();
            containerRegistry.RegisterSingleton<IMilkController, MilkController>();
            containerRegistry.RegisterSingleton<ProjectMWrapper, ProjectMWrapper>();
            containerRegistry.RegisterSingleton<IPresetController, PresetController>();
            containerRegistry.RegisterSingleton<IStateManager, StateManager>();
            containerRegistry.RegisterSingleton<IPresetTimerFactory, PresetTimerFactory>();
            containerRegistry.RegisterSingleton<IPresetSequencerFactory, PresetSequencerFactory>();

            containerRegistry.RegisterSingleton<IPresetProvider, PresetProvider>();
            containerRegistry.RegisterSingleton<IPresetLibraryProvider, PresetLibraryProvider>();
            containerRegistry.RegisterSingleton<ITagProvider, TagProvider>();

            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();

            containerRegistry.RegisterDialog<SelectPresetDialog, SelectPresetDialogModel>();
            containerRegistry.RegisterDialog<ConfigurationDialog, ConfigurationDialogModel>();
            containerRegistry.RegisterDialog<TagEditDialog, TagEditDialogModel>();
            containerRegistry.RegisterDialog<NewTagDialog, NewTagDialogModel>();

            containerRegistry.RegisterSingleton<IIpcHandler, IpcHandler>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
