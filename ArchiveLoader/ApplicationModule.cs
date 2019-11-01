using ArchiveLoader.Interfaces;
using ArchiveLoader.Logging;
using ArchiveLoader.Platform;
using Prism.Ioc;
using Prism.Modularity;

namespace ArchiveLoader {
    class ApplicationModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.Register<IEnvironment, OperatingEnvironment>();
            containerRegistry.Register<IFileWriter, JsonObjectWriter>();
            containerRegistry.Register<IPlatformLog, SeriLogAdapter>();
            containerRegistry.Register<IPreferences, PreferencesManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
