using Caliburn.Micro;
using Prism.Ioc;
using Prism.Modularity;
using ReusableBits.Mvvm.VersionSpinner;
using TuneArchiver.Interfaces;
using TuneArchiver.Logging;
using TuneArchiver.Models;
using TuneArchiver.Platform;

namespace TuneArchiver {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();

            containerRegistry.RegisterSingleton<IEnvironment, OperatingEnvironment>();
            containerRegistry.RegisterSingleton<IFileWriter, JsonObjectWriter>();
            containerRegistry.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
            containerRegistry.RegisterSingleton<IPreferences, PreferencesManager>();
            containerRegistry.RegisterSingleton<IPlatformDialogService, PlatformDialogService>();

            containerRegistry.Register<IArchiveBuilder, ArchiveBuilder>();
            containerRegistry.Register<IDirectoryScanner, DirectoryScanner>();
            containerRegistry.Register<ISetCreator, SetCreator>();
            containerRegistry.Register<IArchiveMedia, ArchiveWiki>();

            containerRegistry.Register<IVersionFormatter, VersionSpinnerViewModel>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
