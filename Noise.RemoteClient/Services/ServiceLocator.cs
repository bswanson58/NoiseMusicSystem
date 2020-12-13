using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Rssdp;

namespace Noise.RemoteClient.Services {
    class ServiceLocator : IServiceLocator {
        private readonly BehaviorSubject<Channel>   mChannelAcquired;
        private SsdpDeviceLocator                   mDeviceLocator;
        private DiscoveredSsdpDevice                mMusicServer;
        private Channel                             mServiceChannel;

        public  IObservable<Channel>                ChannelAcquired => mChannelAcquired;

        public ServiceLocator() {
            mChannelAcquired = new BehaviorSubject<Channel>( null );
        }

        public void StartServiceLocator() {
            StopServiceLocator();

            mDeviceLocator = new SsdpDeviceLocator { NotificationFilter = "uuid:NoiseMusicServer" };

            mDeviceLocator.DeviceAvailable += OnDeviceAvailable;
            mDeviceLocator.DeviceUnavailable += OnDeviceUnavailable;

            mDeviceLocator.StartListeningForNotifications();

            mDeviceLocator.SearchAsync();
        }

        public void StopServiceLocator() {
            mDeviceLocator?.Dispose();
            mDeviceLocator = null;
        }

        private void OnDeviceAvailable( object sender, DeviceAvailableEventArgs e ) {
            if( e.IsNewlyDiscovered ) {
                mMusicServer = e.DiscoveredDevice;

                CreateServiceChannel( mMusicServer );
            }
        }

        private async void CreateServiceChannel( DiscoveredSsdpDevice forDevice ) {
            await StopServiceChannel();

            var serverAddress = $"{forDevice.DescriptionLocation.Host}:{forDevice.DescriptionLocation.Port}";
//            var options = new [] {new ChannelOption( "grpc.max_receive_message_length", 16 * 1024 * 1024) };

            mServiceChannel = new Channel( serverAddress, ChannelCredentials.Insecure ); //, options );

            mChannelAcquired.OnNext( mServiceChannel );
        }

        private async Task StopServiceChannel() {
            if( mServiceChannel != null ) {
                await mServiceChannel.ShutdownAsync();

                mServiceChannel = null;
            }
        }

        private async void OnDeviceUnavailable( object sender, DeviceUnavailableEventArgs e ) {
            await StopServiceChannel();
        }
    }
}
