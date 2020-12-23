using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteClient.Services {
    class QueuePlayProvider : BaseProvider<QueueControl.QueueControlClient>, IQueuePlayProvider {
        private readonly Subject<QueuedItem>    mItemQueued;
        private readonly IPlatformLog           mLog;

        public  IObservable<QueuedItem>         ItemQueued => mItemQueued;

        public QueuePlayProvider( IServiceLocator serviceLocator, IHostInformationProvider hostProvider, IPlatformLog log ) : 
            base( serviceLocator, hostProvider ) {
            mLog = log;

            mItemQueued = new Subject<QueuedItem>();
        }

        public async Task<QueueControlResponse> Queue( TrackInfo track ) {
            var client = Client;

            if( client != null ) {
                try {
                    var retValue = await client.AddTrackAsync( new AddQueueRequest{ ItemId = track.TrackId });

                    if( retValue.Success ) {
                        mItemQueued.OnNext( new QueuedItem( track ));
                    }

                    return retValue;
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueTrack", ex );
                }
            }

            return default;
        }

        public async Task<QueueControlResponse> Queue( AlbumInfo album ) {
            var client = Client;

            if( client != null ) {
                try {
                    var retValue = await client.AddAlbumAsync( new AddQueueRequest { ItemId = album.AlbumId });

                    if( retValue.Success ) {
                        mItemQueued.OnNext( new QueuedItem( album ));
                    }

                    return retValue;
                }
                catch( Exception ex ) {
                    mLog.LogException( "QueueAlbum", ex );
                }
            }

            return default;
        }
    }
}
