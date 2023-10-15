using Noise.Hass.Context;
using Noise.Hass.Hass;
using Noise.Hass.Mqtt;
using Prism.Ioc;
using Prism.Modularity;

// ReSharper disable IdentifierTypo

namespace Noise.Hass {
    public class HassModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.Register<IHassIntegrationManager, HassIntegrationManager>();
            containerRegistry.Register<IHassContextProvider, HassContextProvider>();
            containerRegistry.Register<IHassMqttManager, HassMqttManager>();
            containerRegistry.Register<IMqttManager, MqttManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
