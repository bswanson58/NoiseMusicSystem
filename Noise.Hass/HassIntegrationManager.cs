using Noise.Hass.Context;
using Noise.Hass.Hass;

// ReSharper disable IdentifierTypo

namespace Noise.Hass {
    public interface IHassIntegrationManager {
        HassMqttParameters      GetHassMqttParameters();
        void                    SetHassMqttParameters( HassMqttParameters parameters );
    }

    public class HassIntegrationManager : IHassIntegrationManager {
        private readonly IHassContextProvider   mContextProvider;

        public HassIntegrationManager( IHassContextProvider contextProvider ) {
            mContextProvider = contextProvider;
        }

        public HassMqttParameters GetHassMqttParameters() =>
            mContextProvider.GetHassMqttParameters();

        public void SetHassMqttParameters( HassMqttParameters parameters ) =>
            mContextProvider.SetHassMqttParameters( parameters );
    }
}
