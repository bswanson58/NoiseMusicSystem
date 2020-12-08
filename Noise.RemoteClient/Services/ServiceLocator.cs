using System;
using Grpc.Core;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Rssdp;

namespace Noise.RemoteClient.Services {
    class ServiceLocator : IServiceLocator {
        private SsdpDeviceLocator       mDeviceLocator;
        private DiscoveredSsdpDevice    mMusicServer;

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

                GetServerVersion( e.DiscoveredDevice.DescriptionLocation );
            }
        }

        private async void GetServerVersion( Uri server ) {
            try {
                var serverAddress = $"{server.Host}:{server.Port}";
                var channel = new Channel( serverAddress, ChannelCredentials.Insecure );
                var client = new HostInformation.HostInformationClient( channel );

                var result = await client.GetHostInformationAsync( new Empty());
            }
            catch( Exception ex ) {
                var str = ex.Message;
            }
        }

        private void OnDeviceUnavailable( object sender, DeviceUnavailableEventArgs e ) { }
    }
}
