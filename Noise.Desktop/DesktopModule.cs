using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Desktop.Models;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Platform;

namespace Noise.Desktop {
    public class DesktopModule : IModule {
        private readonly IUnityContainer		mContainer;

        public DesktopModule( IUnityContainer container ) {
            mContainer = container;
        }

        public void Initialize() {
            mContainer.RegisterType<INoiseWindowManager, WindowManager>( new ContainerControlledLifetimeManager());

            mContainer.RegisterType<IIpcManager, IpcManager>( new ContainerControlledLifetimeManager());
            mContainer.RegisterType<IIpcHandler, IpcHandler>( new ContainerControlledLifetimeManager());
        }
    }
}
