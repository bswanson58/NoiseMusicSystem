using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class TransportProvider : BaseProvider<TransportControl.TransportControlClient>, ITransportProvider {
        private readonly IPlatformLog                           mLog;
        private readonly BehaviorSubject<TransportInformation>  mTransportStatus;
        private AsyncServerStreamingCall<TransportInformation>  mTransportStatusStream;
        private CancellationTokenSource                         mTransportStatusStreamCancellation;

        public  IObservable<TransportInformation>               TransportStatus => mTransportStatus;

        public TransportProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) :
            base( serviceLocator, hostProvider  ) {
            mLog = log;

            mTransportStatus = new BehaviorSubject<TransportInformation>( new TransportInformation());
        }

        public async Task<TransportCommandResponse> Play() {
            if( Client != null ) {
                try {
                    return await Client.StartPlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( Play ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> Pause() {
            if( Client != null ) {
                try {
                    return await Client.PausePlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( Pause ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> Stop() {
            if( Client != null ) {
                try {
                    return await Client.StopPlayAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( Stop ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayPrevious() {
            if( Client != null ) {
                try {
                    return await Client.PlayPreviousAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( PlayPrevious ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> PlayNext() {
            if( Client != null ) {
                try {
                    return await Client.PlayNextAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( PlayNext ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> ReplayTrack() {
            if( Client != null ) {
                try {
                    return await Client.ReplayTrackAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( ReplayTrack ), ex );
                }
            }

            return default;
        } 
        
        public async void StartTransportStatusRequests() {
            StopTransportStatusRequests();

            if( Client != null ) {
                try {
                    mTransportStatusStreamCancellation = new CancellationTokenSource();
                    mTransportStatusStream = Client.StartTransportStatus( new TransportControlEmpty(), cancellationToken: mTransportStatusStreamCancellation.Token );
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( StartTransportStatusRequests ), ex );
                }

                if( mTransportStatusStream != null ) {
                    using( mTransportStatusStream ) {
                        try {
                            if( mTransportStatusStreamCancellation?.IsCancellationRequested == false ) {
                                while( await mTransportStatusStream.ResponseStream.MoveNext( mTransportStatusStreamCancellation.Token )) {
                                    PublishTransportStatus( mTransportStatusStream.ResponseStream.Current );
                                }
                            }
                        }
                        catch( RpcException ex ) {
                            if( ex.StatusCode != StatusCode.Cancelled ) {
                                mLog.LogException( "StartTransportStatusRequests:RpcException", ex );
                            }
                        }
                        catch( Exception ex ) {
                            mLog.LogException( nameof( StartTransportStatusRequests ), ex );
                        }
                    }
                }
            }
        }

        private void PublishTransportStatus( TransportInformation status ) {
            if( status != null ) {
                try {
                    mTransportStatus.OnNext( status );
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( PublishTransportStatus ), ex );
                }
            }
        }

        public void StopTransportStatusRequests() {
            mTransportStatusStreamCancellation?.Cancel();
            mTransportStatusStreamCancellation = null;
        }

        public async Task<VolumeLevelInformation> GetVolumeLevel() {
            if( Client != null ) {
                try {
                    return await Client.GetVolumeLevelAsync( new TransportControlEmpty());
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( PlayNext ), ex );
                }
            }

            return default;
        }

        public async Task<VolumeLevelInformation> SetVolumeLevel( int volumeLevel ) {
            if( Client != null ) {
                try {
                    return await Client.SetVolumeLevelAsync( new VolumeLevelInformation { Success = true, 
                                                                                          VolumeLevel = Math.Max( 0, Math.Min( 100,  volumeLevel ))
                    });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( PlayNext ), ex );
                }
            }

            return default;
        }

        public async Task<TransportCommandResponse> OffsetPlaybackPosition( int bySeconds ) {
            if( Client != null ) {
                try {
                    return await Client.OffsetPlaybackPositionAsync( new TransportPositionRequest{ PositionOffsetSeconds = bySeconds });
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( OffsetPlaybackPosition ), ex );
                }
            }

            return default;
        }
    }
}
