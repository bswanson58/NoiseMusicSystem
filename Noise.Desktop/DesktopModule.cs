using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Platform;

namespace Noise.Desktop {
    public class DesktopModule : IModule {
        private readonly IUnityContainer		mContainer;
        private readonly NoiseCorePreferences	mPreferences;

        public DesktopModule( IUnityContainer container, NoiseCorePreferences preferences ) {
            mContainer = container;
            mPreferences = preferences;
        }

        public void Initialize() {
            mContainer.RegisterType<INoiseWindowManager, WindowManager>( new ContainerControlledLifetimeManager());

            mContainer.RegisterType<IIpcHandler, IpcHandler>( new ContainerControlledLifetimeManager());
        }
    }
}
