using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using Noise.Hass.Context;
using Noise.Hass.Mqtt;
using Noise.Hass.Support;
using Noise.Infrastructure.Interfaces;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Hass {
    public interface IHassMqttManager : IDisposable { }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class HassMqttManager : IHassMqttManager {
        private readonly IMqttManager               mMqttManager;
        private readonly IHassContextProvider       mContextProvider;
        private readonly IApplicationLog            mLog;
        private DateTime                            mLastAvailableAnnouncementFailedLogged;
        private IDisposable                         mStatusSubscription;
        private CancellationTokenSource             mTokenSource;
        private Task                                mProcessTask;

        public HassMqttManager( IMqttManager mqttManager, IHassContextProvider contextProvider, IApplicationLog log ) {
            mMqttManager = mqttManager;
            mContextProvider = contextProvider;
            mLog = log;

            mLastAvailableAnnouncementFailedLogged = DateTime.MinValue;

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

        private async Task ShutdownAsync() {
            await StopProcessing();

            await AnnounceAvailabilityAsync( true );

            mStatusSubscription?.Dispose();
            mStatusSubscription = null;
        }

        private async Task Process( CancellationToken cancelToken ) {
            while(!cancelToken.IsCancellationRequested ) {
                try {
                    if(( cancelToken.IsCancellationRequested ) ||
                       ( mMqttManager.Status != MqttStatus.Connected )) {
                        continue;
                    }

                    await AnnounceAvailabilityAsync();

                    await Task.Delay( TimeSpan.FromSeconds( 30 ), cancelToken );
                }
                catch( Exception ex ) {
                    mLog.LogException( "Error while announcing availability.", ex );
                }
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

                    var result = await mMqttManager.PublishAsync( messageBuilder.Build());

                    result.Switch(
                        _ => { },
                        ex => mLog.LogException( "Error when announcing availability.", ex )
                    );
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
            await ShutdownAsync();
        }
    }
}
