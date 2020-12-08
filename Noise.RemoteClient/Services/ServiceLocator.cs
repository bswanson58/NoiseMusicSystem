using System;
using System.Threading.Tasks;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Rssdp;

namespace Noise.RemoteClient.Services {
    class ServiceLocator : IServiceLocator {
        private SsdpDeviceLocator       mDeviceLocator;
        private DiscoveredSsdpDevice    mMusicServer;
        private Channel                 mServiceChannel;

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
                GetServerVersion();
            }
        }

        private async void CreateServiceChannel( DiscoveredSsdpDevice forDevice ) {
            await StopServiceChannel();

            var serverAddress = $"{forDevice.DescriptionLocation.Host}:{forDevice.DescriptionLocation.Port}";

            mServiceChannel = new Channel( serverAddress, ChannelCredentials.Insecure );
        }

        private async Task StopServiceChannel() {
            if( mServiceChannel != null ) {
                await mServiceChannel.ShutdownAsync();

                mServiceChannel = null;
            }
        }

        private async void GetServerVersion() {
            try {
                if( mServiceChannel != null ) {
                    var client = new HostInformation.HostInformationClient( mServiceChannel );

                    var result = await client.GetHostInformationAsync( new Empty());
                }
            }
            catch( Exception ex ) {
                var str = ex.Message;
            }
        }

        private async void OnDeviceUnavailable( object sender, DeviceUnavailableEventArgs e ) {
            await StopServiceChannel();
        }
    }
}
