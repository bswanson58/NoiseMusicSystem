using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Noise.RemoteServer.Interfaces;
using Rssdp;

namespace Noise.RemoteServer.Discovery {
    class DiscoveryService : IDiscoveryService {
        private const int				cDiscoveryPort = 6502;
        private const int				cServicePort = 6503;

        private readonly ISsdpLogger    mDiscoveryLogger;
        private SsdpDevicePublisher     mPublisher;
        private SsdpRootDevice          mDeviceDefinition;

        public DiscoveryService( DiscoveryLogger discoveryLogger ) {
            mDiscoveryLogger = discoveryLogger;
        }

        public void StartDiscoveryService() {
            mPublisher = new SsdpDevicePublisher(
                new Rssdp.Infrastructure.SsdpCommunicationsServer(
                    new SocketFactory( GetLocalIpAddress()), cDiscoveryPort ), mDiscoveryLogger ) {
                NotificationBroadcastInterval = TimeSpan.FromMinutes( 3 )
            };

            mDeviceDefinition = CreateServiceDefinition();
            mPublisher.StandardsMode = SsdpStandardsMode.Relaxed;
            mPublisher.AddDevice( mDeviceDefinition );
        }

        public void StopDiscoveryService() {
            if( mPublisher != null ) {
                if( mDeviceDefinition != null ) {
                    mPublisher.RemoveDevice( mDeviceDefinition );
                    mDeviceDefinition = null;
                }

                mPublisher.Dispose();
                mPublisher = null;
            }
        }

        private SsdpRootDevice CreateServiceDefinition() {
            return new SsdpRootDevice {
                CacheLifetime = TimeSpan.FromMinutes( 3 ),
                Location = new Uri( $"http://{GetLocalIpAddress()}:{cServicePort}"),
                DeviceTypeNamespace = "NoiseMusicSystem",
                DeviceType = "NoiseMusicServer",
                FriendlyName = "Noise Music System",
                Manufacturer = "Secret Squirrel Software",
                ModelName = "Noise.Desktop",
                Uuid = "NoiseMusicServer" // This must be a globally unique value that survives reboots etc. Get from storage or embedded hardware etc.
            };
        }

        public string GetLocalIpAddress() {
            var mostSuitableIp = default( UnicastIPAddressInformation );
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        
            foreach (var network in networkInterfaces) {
                if( network.OperationalStatus != OperationalStatus.Up ) {
                    continue;
                }

                var properties = network.GetIPProperties();

                if( properties.GatewayAddresses.Count == 0 ) {
                    continue;
                }

                foreach( var address in properties.UnicastAddresses ) {
                    if( address.Address.AddressFamily != AddressFamily.InterNetwork ) {
                        continue;
                    }

                    if( IPAddress.IsLoopback( address.Address )) {
                        continue;
                    }

                    if(!address.IsDnsEligible ) {
                        if( mostSuitableIp == null ) {
                            mostSuitableIp = address;
                        }

                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if( address.PrefixOrigin != PrefixOrigin.Dhcp ) {
                        if(( mostSuitableIp == null ) ||
                           (!mostSuitableIp.IsDnsEligible)) {
                              mostSuitableIp = address;
                        }

                        continue;
                    }

                    return address.Address.ToString();
                }
            }

            return mostSuitableIp != null ? mostSuitableIp.Address.ToString() : String.Empty;
        }
    }
}
