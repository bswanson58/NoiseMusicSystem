using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Noise.Hass.Context;
using Noise.Hass.Dto;
using Noise.Hass.Mqtt;
using Noise.Infrastructure.Interfaces;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Handlers {
    public interface ICommandHandler { }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CommandHandler : ICommandHandler, IDisposable {
        private readonly IMqttManager           mMqttManager;
        private readonly IAudioController       mAudioController;
        private readonly IPlayController        mPlayController;
        private readonly IPlayQueue             mPlayQueue;
        private readonly IApplicationLog        mLog;
        private IHassClientContext              mContext;
        private MqttStatus                      mMqttStatus;
        private IDisposable                     mContextSubscription;
        private IDisposable                     mMessageSubscription;
        private IDisposable                     mStatusSubscription;

        public CommandHandler( IMqttManager mqttManager, IPlayController playController, IPlayQueue playQueue,
                               IAudioController audioController, IHassContextProvider contextProvider, IApplicationLog log ) {
            mMqttManager = mqttManager;
            mAudioController = audioController;
            mPlayController = playController;
            mPlayQueue = playQueue;
            mLog = log;

            mContext = null;

            mContextSubscription = contextProvider.OnContextChanged.Subscribe( OnContextChanged );
            mStatusSubscription = mMqttManager.OnStatusChanged.Subscribe( OnStatusChanged );
            mMessageSubscription = mMqttManager.OnMessageReceived.Subscribe( OnMessageReceived );
        }

        private void OnStatusChanged( MqttStatus status ) {
            mMqttStatus = status;

            Subscribe();
        }

        private void OnContextChanged( IHassClientContext context ) {
            mContext = context;

            Subscribe();
        }

        private async void Subscribe() {
            if( mContext != null ) {
                if( mMqttStatus.Equals( MqttStatus.Connected )) {
                    await mMqttManager.SubscribeAsync( mContext.DeviceMessageSubscriptionTopic());
                }
                else {
                    await mMqttManager.UnsubscribeAsync( mContext.DeviceMessageSubscriptionTopic());
                }
            }
        }

        private void OnMessageReceived( MqttMessage message ) {
            if(!String.IsNullOrWhiteSpace( message?.Topic )) {
                var topicParts = message.Topic.Split( '/' );

                if(( topicParts.Length > 2 ) &&
                   ( topicParts[1].Equals( mContext.DeviceConfiguration.Name ))) {
                    if( topicParts[2].ToLower().Equals( "command" )) {
                        var command = JsonConvert.DeserializeObject<CommandDto>( message.Payload );

                        if( command != null ) {
                            switch ( command.Command.ToLower()) {
                                case "play":
                                    ExecuteTransportAction( () => { if( mPlayController.CanPlay ) mPlayController.Play(); });
                                    break;

                                case "pause":
                                    ExecuteTransportAction( () => { if( mPlayController.CanPause ) mPlayController.Pause(); });
                                    break;

                                case "previous":
                                    ExecuteTransportAction( () => { if( mPlayController.CanPlayPreviousTrack ) mPlayController.PlayPreviousTrack(); });
                                    break;

                                case "next":
                                    ExecuteTransportAction( () => { if( mPlayController.CanPlayNextTrack ) mPlayController.PlayNextTrack(); });
                                    break;

                                case "stop":
                                    ExecuteTransportAction( () => { if( mPlayController.CanStop ) mPlayController.Stop(); });
                                    break;

                                case "seek":
                                    Seek( command.Parameter );
                                    break;

                                case "repeat":
                                    ExecuteTransportAction( () => { if( mPlayController.CurrentTrack != null ) mPlayQueue.PlayingTrackReplayCount = 1; });
                                    break;
                                
                                case "volume":
                                    SetVolumeLevel( command.Parameter );
                                    break;
                                
                                case "mute":
                                    SetMuteState( command.Parameter );
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteTransportAction( Action transportAction ) { 
            Task.Run( () => {
                try {
                    transportAction();
                }
                catch( Exception ex ) {
                    mLog.LogException( "Executing Transport Command from HA", ex );
                }
            });
        }

        private void SetVolumeLevel( string volumeString ) {
            var volumeLevel = Convert.ToDouble( volumeString );

            mAudioController.Volume = Math.Min( 100, Math.Max( 0, volumeLevel ));
        }

        private void SetMuteState( string enable ) {
            if(!String.IsNullOrWhiteSpace( enable )) {
                var muteState = enable.ToLower().Equals( "true" );

                mAudioController.Mute = muteState;
            }
        }

        private void Seek( string seekPosition ) {
            if(!String.IsNullOrWhiteSpace( seekPosition )) {
                var seekOffset = Convert.ToDouble( seekPosition );

                if( mPlayController.CanStop ) {
                    var timeSpan = TimeSpan.FromSeconds( seekOffset );

                    if( mPlayController.TrackEndPosition < timeSpan.Ticks ) {
                        timeSpan = TimeSpan.FromTicks( mPlayController.TrackEndPosition );
                    }

                    mPlayController.PlayPosition = timeSpan.Ticks;
                }
            }
        }

        public void Dispose() {
            mContextSubscription?.Dispose();
            mContextSubscription = null;

            mMessageSubscription?.Dispose();
            mMessageSubscription = null;

            mStatusSubscription?.Dispose();
            mStatusSubscription = null;
        }
    }
}
