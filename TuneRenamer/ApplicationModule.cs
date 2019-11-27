using Prism.Ioc;
using Prism.Modularity;
using TuneRenamer.Interfaces;
using TuneRenamer.Logging;
using TuneRenamer.Platform;

namespace TuneRenamer {
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
