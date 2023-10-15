using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using Noise.Hass.Context;
using Noise.Hass.Mqtt;
using Noise.Hass.Support;
using Noise.Infrastructure.Interfaces;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Hass {
    public interface IMqttMessageHandler {
        bool    ProcessMessage( MqttMessage message );
    }

    public interface IHassMqttManager : IDisposable {
        void    RegisterMessageHandler( IMqttMessageHandler handler );
        void    RevokeMessageHandler( IMqttMessageHandler handler );
    }

    public class HassMqttManager : IHassMqttManager {
        private readonly IMqttManager               mMqttManager;
        private readonly IHassContextProvider       mContextProvider;
        private readonly IApplicationLog            mLog;
        private readonly List<IMqttMessageHandler>  mMessageHandlers;
        private DateTime                            mLastAvailableAnnouncementFailedLogged;
        private IDisposable                         mMessageSubscription;
        private IDisposable                         mStatusSubscription;
        private CancellationTokenSource             mTokenSource;
        private Task                                mProcessTask;

        public HassMqttManager( IMqttManager mqttManager, IHassContextProvider contextProvider, IApplicationLog log ) {
            mMqttManager = mqttManager;
            mContextProvider = contextProvider;
            mLog = log;

            mMessageHandlers = new List<IMqttMessageHandler>();
            mLastAvailableAnnouncementFailedLogged = DateTime.MinValue;

            mMessageSubscription = mMqttManager.OnMessageReceived.Subscribe( OnMessageReceived );
            mStatusSubscription = mMqttManager.OnStatusChanged.Subscribe( OnMqttStatusChanged );
        }

        private async void OnMqttStatusChanged( MqttStatus status ) {
            if( status.Equals( MqttStatus.Connected )) {
                await StartProcessing();
            }
            else {
                await StopProcessing();
            }
        }

        private async Task StartProcessing() {
            await StopProcessing();

            mTokenSource = new CancellationTokenSource();
            mProcessTask = Task.Run(() => Process( mTokenSource.Token ), mTokenSource.Token );
        }

        private async Task StopProcessing() {
            mTokenSource?.Cancel();

            if( mProcessTask != null ) {
                await mProcessTask;

                mProcessTask?.Dispose();
                mProcessTask = null;
            }

            mTokenSource?.Dispose();
            mTokenSource = null;
        }

        public void RegisterMessageHandler( IMqttMessageHandler handler ) {
            RevokeMessageHandler( handler );

            mMessageHandlers.Add( handler );
        }

        public void RevokeMessageHandler( IMqttMessageHandler handler ) {
            if( mMessageHandlers.Contains( handler )) {
                mMessageHandlers.Remove( handler );
            }
        }

        private void OnMessageReceived( MqttMessage message ) {
            if( message.Topic.EndsWith( Constants.Subscribe )) {
                foreach( var handler in mMessageHandlers ) {
                    if( handler.ProcessMessage( message )) {
                        break;
                    }
                }
            }
        }

        public async Task ShutdownAsync() {
            mTokenSource?.Cancel();

            mMessageSubscription?.Dispose();
            mMessageSubscription = null;

            if( mProcessTask != null ) {
                await mProcessTask;

                mProcessTask?.Dispose();
            }

            await AnnounceAvailabilityAsync( true );
        }

        private async Task Process( CancellationToken cancelToken ) {
            var subscriptionRequested = false;

            while(!cancelToken.IsCancellationRequested ) {
                try {
                    if(( cancelToken.IsCancellationRequested ) ||
                       ( mMqttManager.Status != MqttStatus.Connected )) {
                        continue;
                    }

                    if(!subscriptionRequested ) {
                        await mMqttManager.SubscribeAsync( mContextProvider.Context.DeviceMessageSubscriptionTopic());

                        subscriptionRequested = true;
                    }

                    await AnnounceAvailabilityAsync();

                    await Task.Delay( TimeSpan.FromSeconds( 30 ), cancelToken );
                }
                catch( Exception ex ) {
                    mLog.LogException( "Error while announcing availability.", ex );
                }
            }

            if( subscriptionRequested ) {
                await mMqttManager.UnsubscribeAsync( mContextProvider.Context.DeviceMessageSubscriptionTopic());
            }
        }

        private async Task AnnounceAvailabilityAsync( bool offline = false ) {
            try {
                if( mMqttManager.IsConnected ) {
                    var topic = mContextProvider.Context.DeviceAvailabilityTopic();
                    var messageBuilder = new MqttApplicationMessageBuilder()
                        .WithTopic( topic )
                        .WithPayload( offline ? Constants.Offline : Constants.Online )
                        .WithRetainFlag( mContextProvider.Context.UseMqttRetainFlag );

                    await mMqttManager.PublishAsync( messageBuilder.Build());
                }
                else {
                    // only log failures once every 5 minutes to minimize log growth
                    if(( DateTime.Now - mLastAvailableAnnouncementFailedLogged ).TotalMinutes < 5 ) {
                        return;
                    }

                    mLastAvailableAnnouncementFailedLogged = DateTime.Now;
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "Error while announcing availability", ex );
            }
        }

        public async void Dispose() {
            mMessageSubscription?.Dispose();
            mMessageSubscription = null;

            mStatusSubscription?.Dispose();
            mStatusSubscription = null;

            await StopProcessing();
        }
    }
}
