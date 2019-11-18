using Album4Matter.Interfaces;
using Album4Matter.Logging;
using Album4Matter.Platform;
using Prism.Ioc;
using Prism.Modularity;

namespace Album4Matter {
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
