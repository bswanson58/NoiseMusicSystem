using Noise.Hass.Context;
using Noise.Hass.Handlers;
using Noise.Hass.Hass;

// ReSharper disable IdentifierTypo

namespace Noise.Hass {
    public interface IHassIntegrationManager {
        HassMqttParameters      GetHassMqttParameters();
        void                    SetHassMqttParameters( HassMqttParameters parameters );
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class HassIntegrationManager : IHassIntegrationManager {
        private readonly IHassContextProvider   mContextProvider;
        private readonly IHassMqttManager       mHassMqttManager;
        private readonly IStatusHandler         mStatusHandler;

        public HassIntegrationManager( IHassContextProvider contextProvider, IHassMqttManager hassMqttManager,
                                       IStatusHandler statusHandler ) {
            mContextProvider = contextProvider;
            mHassMqttManager = hassMqttManager;
            mStatusHandler = statusHandler;
        }

        public HassMqttParameters GetHassMqttParameters() =>
            mContextProvider.GetHassMqttParameters();

        public void SetHassMqttParameters( HassMqttParameters parameters ) =>
            mContextProvider.SetHassMqttParameters( parameters );
    }
}
