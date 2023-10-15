using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using Noise.Hass.Discovery;
using Noise.Hass.Hass;
using Noise.Hass.Mqtt;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Context {
    public interface IHassContextProvider {
        IHassClientContext              Context { get; }
        IObservable<IHassClientContext> OnContextChanged { get; }

        HassMqttParameters  GetHassMqttParameters();
        void                SetHassMqttParameters( HassMqttParameters parameters );
    }

    public class HassContextProvider : IHassContextProvider {
        private readonly IPreferences       mPreferences;

        private readonly BehaviorSubject<IHassClientContext>    mContextSubject;

        public  IHassClientContext              Context { get; private set; }
        public  IObservable<IHassClientContext> OnContextChanged => mContextSubject.AsObservable();

        public HassContextProvider( IPreferences preferences ) {
            mPreferences = preferences;

            Context = CreateContext();

            mContextSubject = new BehaviorSubject<IHassClientContext>( Context );
        }

        private void UpdateContext() {
            Context = CreateContext();

            mContextSubject.OnNext( Context );
        }

        private IHassClientContext CreateContext() {
            var hassParameters = mPreferences.Load<HassParameters>();
            var mqttParameters = mPreferences.Load<MqttParameters>();

            return new HassClientContext( mqttParameters, hassParameters, GetDeviceConfiguration( hassParameters ));
        }

        private DeviceConfigModel GetDeviceConfiguration( HassParameters hassParameters ) {
            var deviceConfiguration = new DeviceConfigModel {
                Manufacturer = Constants.CompanyName,
                Name = GetConfiguredDeviceName( hassParameters.DeviceName ),
                Identifiers = GetConfiguredDeviceName( hassParameters.ClientIdentifier ),
                Model = hassParameters.Model,
                SoftwareVersion = hassParameters.Version
            };

            return deviceConfiguration;
        }

        public HassMqttParameters GetHassMqttParameters() {
            var deviceParameters = mPreferences.Load<HassParameters>();
            var mqqtParameters = mPreferences.Load<MqttParameters>();

            return new HassMqttParameters {
                MqttEnabled = mqqtParameters.MqttEnabled,
                ServerAddress = mqqtParameters.ServerAddress,
                UserName = mqqtParameters.UserName,
                Password = mqqtParameters.Password,
                UseRetainFlag = mqqtParameters.UseRetainFlag,
                DeviceName = deviceParameters.DeviceName,
                ClientIdentifier = deviceParameters.ClientIdentifier
            };
        }

        public void SetHassMqttParameters( HassMqttParameters parameters ) {
            var mqttParameters = mPreferences.Load<MqttParameters>();

            mqttParameters.MqttEnabled = parameters.MqttEnabled;
            mqttParameters.ServerAddress = parameters.ServerAddress;
            mqttParameters.UserName = parameters.UserName;
            mqttParameters.Password = parameters.Password;
            mqttParameters.UseRetainFlag = parameters.UseRetainFlag;
            mPreferences.Save( mqttParameters );

            var hassParameters = mPreferences.Load<HassParameters>();

            hassParameters.DeviceName = parameters.DeviceName;
            hassParameters.ClientIdentifier = parameters.ClientIdentifier;

            mPreferences.Save( hassParameters );

            UpdateContext();
        }

        private static string GetConfiguredDeviceName( string clientName ) =>
            string.IsNullOrEmpty( clientName )
                ? GetSafeDeviceName() 
                : GetSafeValue( clientName );

        /// <summary>
        /// Returns a safe version of this machine's name
        /// </summary>
        /// <returns></returns>
        private static string GetSafeDeviceName() => 
            GetSafeValue( Environment.MachineName );

        /// <summary>
        /// Returns a safe version of the provided value
        /// </summary>
        /// <returns></returns>
        private static string GetSafeValue( string value ) {
            var val = Regex.Replace( value, @"[^a-zA-Z0-9_\-_\s]", "_" );

            return val.Replace( " ", String.Empty );
        }
    }
}
