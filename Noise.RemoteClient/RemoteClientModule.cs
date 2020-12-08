using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Services;
using Prism.Ioc;

namespace Noise.RemoteClient {
    public class RemoteClientModule {
        public void RegisterServices( IContainerRegistry container ) {
            container.Register<IServiceLocator, ServiceLocator>();
        }
    }
}
