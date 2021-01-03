using System.Collections.Generic;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class TransportStatus {
        private readonly List<TransportTagInfo> mCurrentTags;
        private ePlayState                      mPlayState;
        private PlayQueueTrack                  mCurrentTrack;
        private long                            mPlayPosition;
        private long                            mTrackLength;
        private double                          mPlayPercentage;

        public TransportStatus() {
            mCurrentTags = new List<TransportTagInfo>();
            mPlayState = ePlayState.Stopped;
        }

        public void UpdateTransportStatus( Events.PlaybackTrackStarted args ) {
            mCurrentTrack = args.Track;
        }

        public void UpdateTransportStatus( Events.PlaybackTrackUpdated args ) {
            mCurrentTrack = args.Track;
        }

        public void UpdateTransportStatus( PlayQueueTrack track ) {
            mCurrentTrack = track;
        }

        public void UpdateTransportStatus( ePlayState toState ) {
            mPlayState = toState;
        }

        public void UpdateTrackTags( IEnumerable<TransportTagInfo> tags ) {
            mCurrentTags.Clear();
            mCurrentTags.AddRange( tags );
        }

        public void UpdateTrackPosition( long position, long trackLength, double playPercentage ) {
            mPlayPosition = position;
            mTrackLength = trackLength;
            mPlayPercentage = playPercentage;
        }

        public TransportInformation CreateTransportInformation() {
            var retValue = new TransportInformation();

            if( mCurrentTrack != null ) {
                retValue.ArtistId = mCurrentTrack.Artist.DbId;
                retValue.ArtistName = mCurrentTrack.Artist.Name;

                retValue.AlbumId = mCurrentTrack.Album.DbId;
                retValue.AlbumName = mCurrentTrack.Album.Name;

                retValue.TrackId = mCurrentTrack.Track.DbId;
                retValue.TrackNumber = mCurrentTrack.Track.TrackNumber;
                retValue.TrackName = mCurrentTrack.Track.Name;
                retValue.VolumeName = mCurrentTrack.Track.VolumeName;

                retValue.Rating = mCurrentTrack.Track.Rating;
                retValue.IsFavorite = mCurrentTrack.Track.IsFavorite;

                retValue.IsStrategyQueued = mCurrentTrack.IsStrategyQueued;
                retValue.IsFaulted = mCurrentTrack.IsFaulted;
            }

            switch( mPlayState ) {
                case ePlayState.StoppedEmptyQueue:
                case ePlayState.Stopped:
                case ePlayState.Stopping:
                    retValue.TransportState = TransportState.Stopped;
                    break;

                case ePlayState.StartPlaying:
                case ePlayState.Playing:
                case ePlayState.PlayNext: 
                case ePlayState.PlayPrevious:
                case ePlayState.Resuming:
                case ePlayState.ExternalPlay:
                    retValue.TransportState = TransportState.Playing;
                    break;

                case ePlayState.Paused:
                case ePlayState.Pausing:
                    retValue.TransportState = TransportState.Paused;
                    break;

                default:
                    retValue.TransportState = TransportState.Unknown;
                    break;
            }

            retValue.PlayPosition = mPlayPosition;
            retValue.TrackLength = mTrackLength;
            retValue.PlayPositionPercentage = mPlayPercentage;

            retValue.Tags.AddRange( mCurrentTags );

            return retValue;
        }
    }
}
