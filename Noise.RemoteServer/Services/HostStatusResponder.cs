using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Grpc.Core;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RemoteServer.Protocol;

namespace Noise.RemoteServer.Services {
    class HostStatusResponder : IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
        private readonly ILibraryConfiguration          mLibraryConfiguration;
        private readonly IEventAggregator               mEventAggregator;
        private IServerStreamWriter<HostStatusResponse> mStatusStream;
        private ServerCallContext                       mCallContext;
        private TaskCompletionSource<bool>              mStatusComplete;

        public HostStatusResponder( ILibraryConfiguration libraryConfiguration, IEventAggregator eventAggregator ) {
            mLibraryConfiguration = libraryConfiguration;
            mEventAggregator = eventAggregator;
        }

        public async Task StartResponder( IServerStreamWriter<HostStatusResponse> stream, ServerCallContext callContext ) {
            mStatusStream = stream;
            mCallContext = callContext;
            mStatusComplete = new TaskCompletionSource<bool>( false );

            PublishStatus();

            mEventAggregator.Subscribe( this );

            await mStatusComplete.Task;
        }

        public void Handle( Events.DatabaseOpened args ) {
            PublishStatus();
        }

        public void Handle( Events.DatabaseClosing args ) {
            PublishStatus();
        }

        private async void PublishStatus() {
            if(!mCallContext.CancellationToken.IsCancellationRequested ) {
                try {
                    var status = 
                        new HostStatusResponse{ LibraryOpen = mLibraryConfiguration.Current != null, 
                                                LibraryName = mLibraryConfiguration.Current != null ? mLibraryConfiguration.Current.LibraryName : String.Empty };
                    await mStatusStream.WriteAsync( status );
                }
                catch( Exception ex ) { }
            }
            else {
                StopResponder();
            }
        }

        public void StopResponder() {
            mEventAggregator.Unsubscribe( this );
            mStatusComplete.SetResult( true );
        }
    }
}
