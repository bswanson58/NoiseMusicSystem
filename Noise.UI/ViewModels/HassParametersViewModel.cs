using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Noise.Hass;
using Noise.Hass.Hass;
using Noise.Hass.Mqtt;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

// ReSharper disable IdentifierTypo

namespace Noise.UI.ViewModels {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class HassParametersViewModel : DialogAwareBase {
        private readonly IHassIntegrationManager    mHassManager;
        private readonly IMqttManager               mMqttManager;
        private bool                                mEnableIntegration;
        private bool                                mUseRetainFlag;
        private string                              mServerAddress;
        private string                              mUserName;
        private string                              mPassword;
        private string                              mDeviceName;
        private string                              mClientIdentifier;
        private IDisposable                         mStatusSubscription;

        public  bool                                IsIntegrationEnabled => mEnableIntegration;
        public  string                              MqttStatus { get; private set; }
        public  string                              MqttMessage { get; private set; }

        public  DelegateCommand                     TestConnection { get; }

        public HassParametersViewModel( IHassIntegrationManager hassManager, IMqttManager mqttManager ) {
            mHassManager = hassManager;
            mMqttManager = mqttManager;

            var parameters = mHassManager.GetHassMqttParameters();

            mEnableIntegration = parameters.MqttEnabled;
            mServerAddress = parameters.ServerAddress;
            mUserName = parameters.UserName;
            mPassword = parameters.Password;
            mUseRetainFlag = parameters.UseRetainFlag;
            mDeviceName = parameters.DeviceName;
            mClientIdentifier = parameters.ClientIdentifier;

            TestConnection = new DelegateCommand( OnTestConnection, CanAccept );

            MqttStatus = String.Empty;
            MqttMessage = String.Empty;

            Title = "Home Assistant Integration";
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            base.OnDialogOpened( parameters );

            mStatusSubscription = mMqttManager.OnStatusChanged
                .ObserveOn( CurrentThreadScheduler.Instance )
                .Subscribe( OnMqttStatusChanged );
        }

        private void OnMqttStatusChanged( MqttStatus status ) {
            switch ( status ) {
                case Hass.Mqtt.MqttStatus.Disconnected:
                    MqttStatus = "Disconnected";
                    break;

                case Hass.Mqtt.MqttStatus.Connected:
                    MqttStatus = "Connected";
                    break;

                case Hass.Mqtt.MqttStatus.Connecting:
                    MqttStatus = "Connecting";
                    break;

                case Hass.Mqtt.MqttStatus.Error:
                    MqttStatus = "Error";
                    break;

                case Hass.Mqtt.MqttStatus.Uninitialized:
                    MqttStatus = "Uninitialized";
                    break;
            }

            MqttMessage = mMqttManager.StatusMessage;

            RaisePropertyChanged( () => MqttStatus );
            RaisePropertyChanged( () => MqttMessage );
        }

        public bool EnableIntegration {
            get => mEnableIntegration;
            set {
                mEnableIntegration = value;

                RaisePropertyChanged(() => EnableIntegration );
                RaisePropertyChanged(() => IsIntegrationEnabled );
                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public string ServerAddress {
            get => mServerAddress;
            set {
                mServerAddress = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public string UserName {
            get => mUserName;
            set {
                mUserName = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public string Password {
            get => mPassword;
            set {
                mPassword = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public string DeviceName {
            get => mDeviceName;
            set {
                mDeviceName = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public string ClientIdentifier {
            get => mClientIdentifier;
            set {
                mClientIdentifier = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        public bool UseRetainFlag {
            get => mUseRetainFlag;
            set {
                mUseRetainFlag = value;

                Ok.RaiseCanExecuteChanged();
                TestConnection.RaiseCanExecuteChanged();
            }
        }

        private void UpdateParameters() {
            var parameters = new HassMqttParameters {
                MqttEnabled = mEnableIntegration,
                ServerAddress = mServerAddress,
                UserName = mUserName,
                Password = mPassword,
                UseRetainFlag = mUseRetainFlag,
                DeviceName = mDeviceName,
                ClientIdentifier = mClientIdentifier
            };

            mHassManager.SetHassMqttParameters( parameters );
        }

        protected override bool CanAccept() =>
            !mEnableIntegration ||
            !String.IsNullOrWhiteSpace( mServerAddress ) &&
            !String.IsNullOrWhiteSpace( mDeviceName ) &&
            !String.IsNullOrWhiteSpace( mClientIdentifier );

        private void OnTestConnection() {
            if( CanAccept()) {
                UpdateParameters();
            }
        }

        protected override void OnOk() {
            UpdateParameters();

            base.OnOk();
        }

        public override void OnDialogClosed() {
            mStatusSubscription?.Dispose();
            mStatusSubscription = null;
        }
    }
}
