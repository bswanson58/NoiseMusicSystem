using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Unity;
using Noise.Service.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceBus;

namespace Noise.Service.Infrastructure {
	public class ServiceInfrastructureModule : IModule {
		private readonly IUnityContainer    mContainer;

		public ServiceInfrastructureModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IServiceBusManager, ServiceBusManager>();
		}
	}
}
