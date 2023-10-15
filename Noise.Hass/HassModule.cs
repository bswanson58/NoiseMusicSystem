using Noise.Hass.Context;
using Noise.Hass.Hass;
using Noise.Hass.Mqtt;
using Prism.Ioc;
using Prism.Modularity;

// ReSharper disable IdentifierTypo

namespace Noise.Hass {
    public class HassModule : IModule {
        public void RegisterTypes( IContainerRegistry containerRegistry ) {
            containerRegistry.RegisterSingleton<IHassIntegrationManager, HassIntegrationManager>();
            containerRegistry.RegisterSingleton<IHassContextProvider, HassContextProvider>();
            containerRegistry.RegisterSingleton<IHassMqttManager, HassMqttManager>();
            containerRegistry.RegisterSingleton<IMqttManager, MqttManager>();
        }

        public void OnInitialized( IContainerProvider containerProvider ) {
        }
    }
}
