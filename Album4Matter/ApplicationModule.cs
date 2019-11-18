using ForRent.Interfaces;
using ForRent.Logging;
using ForRent.Platform;
using Prism.Ioc;
using Prism.Modularity;

namespace ForRent {
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
