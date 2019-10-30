using Prism.Ioc;
using Prism.Modularity;
using TuneArchiver.Interfaces;
using TuneArchiver.Logging;
using TuneArchiver.Models;
using TuneArchiver.Platform;
using Unity.Lifetime;

namespace TuneArchiver {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IEnvironment, OperatingEnvironment>();
            containerRegistry.RegisterSingleton<IFileWriter, JsonObjectWriter>();
            containerRegistry.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
            containerRegistry.RegisterSingleton<IPreferences, PreferencesManager>();
            containerRegistry.RegisterSingleton<IPlatformDialogService, PlatformDialogService>();

            containerRegistry.Register<IArchiveBuilder, ArchiveBuilder>();
            containerRegistry.Register<IDirectoryScanner, DirectoryScanner>();
            containerRegistry.Register<ISetCreator, SetCreator>();
            containerRegistry.Register<IArchiveMedia, ArchiveWiki>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
