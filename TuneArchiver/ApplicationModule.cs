using Prism.Ioc;
using Prism.Modularity;
using TuneArchiver.Interfaces;
using TuneArchiver.Logging;
using TuneArchiver.Models;
using TuneArchiver.Platform;

namespace TuneArchiver {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();

            containerRegistry.Register<IArchiveBuilder, ArchiveBuilder>();
            containerRegistry.Register<IDirectoryScanner, DirectoryScanner>();
            containerRegistry.Register<ISetCreator, SetCreator>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
