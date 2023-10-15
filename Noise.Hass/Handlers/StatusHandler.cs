using System;
using Caliburn.Micro;
using Noise.Hass.Context;
using Noise.Hass.Dto;
using Noise.Hass.Mqtt;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Handlers {
    public interface IStatusHandler { }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class StatusHandler : IHandle<Events.PlaybackTrackChanged>, IHandle<Events.PlaybackInfoChanged>,
                                   IHandle<Events.PlaybackTrackStarted>, IHandle<Events.PlaybackTrackUpdated>,
                                   IHandle<Events.TrackUserUpdate>, IStatusHandler, IDisposable {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IAudioController       mAudioController;
        private readonly IPlayController        mPlayController;
        private readonly IMqttManager           mMqttManager;
        private readonly StatusDto              mStatus;
        private IHassClientContext              mContext;
        private IDisposable                     mContextSubscription;
        private IDisposable                     mPlayStateChangedSubscription;

        public StatusHandler( IPlayController playController, IMqttManager mqttManager, IHassContextProvider contextProvider,
                              IAudioController audioController, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mAudioController = audioController;
            mPlayController = playController;
            mMqttManager = mqttManager;

            mStatus = new StatusDto();
            mStatus.PlayState = "not_playing";

            mContextSubscription = contextProvider.OnContextChanged.Subscribe( OnContextChanged );
            mPlayStateChangedSubscription = mPlayController.PlayStateChange.Subscribe( OnPlayStateChanged );

            mEventAggregator.Subscribe( this );
        }

        private void OnContextChanged( IHassClientContext context ) {
            mContext = context;

            UpdateStatus( mStatus );
        }

        private void OnPlayStateChanged( ePlayState toState ) {
            switch( toState ) {
                case ePlayState.StoppedEmptyQueue:
                case ePlayState.Stopped:
                case ePlayState.Stopping:
                case ePlayState.Paused:
                case ePlayState.Pausing:
                    mStatus.PlayState = "not_playing";
                    break;

                case ePlayState.StartPlaying:
                case ePlayState.Playing:
                case ePlayState.PlayNext: 
                case ePlayState.PlayPrevious:
                case ePlayState.Resuming:
                case ePlayState.ExternalPlay:
                    mStatus.PlayState = "playing";
                    break;

                default:
                    mStatus.PlayState = "not_playing";
                    break;
            }

            UpdatePlayingStatus();
        }

        public void Handle( Events.PlaybackTrackChanged args ) {
            UpdatePlayingStatus();
        }

        public void Handle( Events.PlaybackInfoChanged args ) {
            UpdatePlayingStatus();
        }

        public void Handle( Events.PlaybackTrackStarted args ) {
            UpdatePlayingTrackInfo( args.Track );
        }

        public void Handle( Events.PlaybackTrackUpdated args ) {
            UpdatePlayingTrackInfo( args.Track );
        }

        public void Handle( Events.TrackUserUpdate args ) {
            UpdatePlayingTrackInfo( args.Track );
        }

        private void UpdatePlayingStatus() {
            mStatus.Position = Convert.ToInt32( TimeSpan.FromTicks( mPlayController.PlayPosition ).TotalSeconds );
            mStatus.Duration = Convert.ToInt32( TimeSpan.FromTicks( mPlayController.TrackEndPosition ).TotalSeconds );
            mStatus.PositionUpdatedAt = DateTime.Now.ToString( "R" ); // RFC 1123 format

            mStatus.Volume = Convert.ToInt32( mAudioController.Volume * 100.0 );

            UpdateStatus( mStatus );
        }

        private void UpdatePlayingTrackInfo( DbTrack fromTrack ) {
            mStatus.TrackName = fromTrack.Name;
            mStatus.TrackNumber = fromTrack.TrackNumber;

            UpdatePlayingStatus();
            UpdateStatus( mStatus );
        }

        private void UpdatePlayingTrackInfo( PlayQueueTrack fromTrack ) {
            mStatus.Artist = fromTrack.Artist.Name;
            mStatus.Album = fromTrack.Album.Name;
            mStatus.TrackName = fromTrack.Track.Name;
            mStatus.TrackNumber = fromTrack.Track.TrackNumber;
            mStatus.Duration = Convert.ToInt32( TimeSpan.FromMilliseconds( fromTrack.Track.DurationMilliseconds ).TotalSeconds );

            UpdatePlayingStatus();
            UpdateStatus( mStatus );
        }

        private void UpdateStatus( StatusDto status ) {
            if( mContext != null ) {
                mMqttManager.PublishAsync( mContext.DeviceStatusTopic(), status );
            }
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );

            mContextSubscription?.Dispose();
            mContextSubscription = null;

            mPlayStateChangedSubscription?.Dispose();
            mPlayStateChangedSubscription = null;
        }
    }
}
