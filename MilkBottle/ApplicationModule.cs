using Caliburn.Micro;
using MilkBottle.Database;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Logging;
using MilkBottle.Models;
using MilkBottle.Models.PresetSequencers;
using MilkBottle.Models.PresetTimers;
using MilkBottle.Models.Sunset;
using MilkBottle.Platform;
using MilkBottle.ViewModels;
using MilkBottle.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
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
            containerRegistry.RegisterSingleton<IIpcManager, IpcManager>();
            containerRegistry.RegisterSingleton<IMilkController, MilkController>();
            containerRegistry.RegisterSingleton<ProjectMWrapper, ProjectMWrapper>();
            containerRegistry.RegisterSingleton<IPresetController, PresetController>();
            containerRegistry.RegisterSingleton<IStateManager, StateManager>();
            containerRegistry.RegisterSingleton<ISyncManager, SyncManager>();
            containerRegistry.RegisterSingleton<IPresetTimerFactory, PresetTimerFactory>();
            containerRegistry.RegisterSingleton<IPresetSequencerFactory, PresetSequencerFactory>();

            containerRegistry.RegisterSingleton<IMoodProvider, MoodProvider>();
            containerRegistry.RegisterSingleton<IPresetProvider, PresetProvider>();
            containerRegistry.RegisterSingleton<IPresetLibraryProvider, PresetLibraryProvider>();
            containerRegistry.RegisterSingleton<IPresetListProvider, PresetListProvider>();
            containerRegistry.RegisterSingleton<IPresetSetProvider, PresetSetProvider>();
            containerRegistry.RegisterSingleton<ISceneProvider, SceneProvider>();
            containerRegistry.RegisterSingleton<ITagProvider, TagProvider>();

            containerRegistry.Register<IApplicationConstants, ApplicationConstants>();
            containerRegistry.Register<ICelestialCalculator, CelestialCalculator>();
            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();

            containerRegistry.RegisterDialog<ConfirmDeleteDialog, ConfirmDeleteDialogModel>();
            containerRegistry.RegisterDialog<SelectPresetDialog, SelectPresetDialogModel>();
            containerRegistry.RegisterDialog<ConfigurationDialog, ConfigurationDialogModel>();
            containerRegistry.RegisterDialog<LightPipeDialog, LightPipeDialogModel>();
            containerRegistry.RegisterDialog<TagEditDialog, TagEditDialogModel>();
            containerRegistry.RegisterDialog<MoodManagementDialog, MoodManagementDialogModel>();
            containerRegistry.RegisterDialog<MoodSelectionDialog, MoodSelectionDialogModel>();
            containerRegistry.RegisterDialog<NewMoodDialog, NewMoodDialogModel>();
            containerRegistry.RegisterDialog<NewTagDialog, NewTagDialogModel>();
            containerRegistry.RegisterDialog<NewSceneDialog, NewSceneDialogModel>();
            containerRegistry.RegisterDialog<NewSetDialog, NewSetDialogModel>();
            containerRegistry.RegisterDialog<SceneWizardDialog, SceneWizardDialogModel>();
            containerRegistry.RegisterDialog<SelectSceneDialog, SelectSceneDialogModel>();

            containerRegistry.RegisterSingleton<IIpcHandler, IpcHandler>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion( RegionNames.ReviewRegion, typeof( PresetEditView ));
            regionManager.RegisterViewWithRegion( RegionNames.ReviewRegion, typeof( TagEditView ));
            regionManager.RegisterViewWithRegion( RegionNames.ReviewRegion, typeof( SetEditView ));
            regionManager.RegisterViewWithRegion( RegionNames.ReviewRegion, typeof( SceneEditView ));
        }
    }
}
