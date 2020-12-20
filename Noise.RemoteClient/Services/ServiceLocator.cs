using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Rssdp;

namespace Noise.RemoteClient.Services {
    class ServiceLocator : IServiceLocator {
        private readonly BehaviorSubject<Channel>   mChannelAcquired;
        private readonly IPlatformLog               mLog;
        private SsdpDeviceLocator                   mDeviceLocator;
        private DiscoveredSsdpDevice                mMusicServer;
        private Channel                             mServiceChannel;

        public  IObservable<Channel>                ChannelAcquired => mChannelAcquired;

        public ServiceLocator( IPlatformLog log ) {
            mLog = log;

            mChannelAcquired = new BehaviorSubject<Channel>( null );
        }

        public void StartServiceLocator() {
            StopServiceLocator();

            try {
                mDeviceLocator = new SsdpDeviceLocator { NotificationFilter = "uuid:NoiseMusicServer" };

                mDeviceLocator.DeviceAvailable += OnDeviceAvailable;
                mDeviceLocator.DeviceUnavailable += OnDeviceUnavailable;

                mDeviceLocator.StartListeningForNotifications();

                mDeviceLocator.SearchAsync();
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( StartServiceLocator ), ex );
            }
        }

        public async void StopServiceLocator() {
            mDeviceLocator?.Dispose();
            mDeviceLocator = null;

            await StopServiceChannel();
        }

        private void OnDeviceAvailable( object sender, DeviceAvailableEventArgs e ) {
            if( e.IsNewlyDiscovered ) {
                mMusicServer = e.DiscoveredDevice;

                CreateServiceChannel( mMusicServer );
            }
        }

        private async void CreateServiceChannel( DiscoveredSsdpDevice forDevice ) {
            await StopServiceChannel();

            try {
                var serverAddress = $"{forDevice.DescriptionLocation.Host}:{forDevice.DescriptionLocation.Port}";
//              var options = new [] {new ChannelOption( "grpc.max_receive_message_length", 16 * 1024 * 1024) };

                mServiceChannel = new Channel( serverAddress, ChannelCredentials.Insecure ); //, options );

                mChannelAcquired.OnNext( mServiceChannel );
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( CreateServiceChannel ), ex );
            }
        }

        private async Task StopServiceChannel() {
            if( mServiceChannel != null ) {
                try {
                    await mServiceChannel.ShutdownAsync();

                    mServiceChannel = null;

                    mChannelAcquired.OnNext( mServiceChannel );
                }
                catch( Exception ex ) {
                    mLog.LogException( nameof( StopServiceChannel ), ex );
                }
            }
        }

        private void OnDeviceUnavailable( object sender, DeviceUnavailableEventArgs e ) {
            mServiceChannel = null;

            try {
                mChannelAcquired.OnNext( mServiceChannel );
            }
            catch( Exception ex ) {
                mLog.LogException( nameof( OnDeviceUnavailable ), ex );
            }
        }
    }
}
