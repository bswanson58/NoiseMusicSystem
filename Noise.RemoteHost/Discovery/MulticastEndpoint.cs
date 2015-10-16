using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Noise.Infrastructure.Interfaces;

namespace Noise.RemoteHost.Discovery {
	public class MulticastEndpoint {
		private readonly IPAddress		mMulticastAddress;
		private readonly int			mMulticastPort;
		private readonly INoiseLog		mLog;
		private bool					mContinueListening;

		public MulticastEndpoint( IPAddress multicastAddress, int port, INoiseLog log ) {
			mMulticastAddress = multicastAddress;
			mMulticastPort = port;
			mLog = log;
		}

		public void StartListener( IRequestResponder responder ) {
			var endPoint = new IPEndPoint( IPAddress.Any, mMulticastPort );
			var udpClient = new UdpClient { ExclusiveAddressUse = false };

			udpClient.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
//			udpClient.Client.MulticastLoopback = false;
			udpClient.Client.Bind( endPoint );
			udpClient.Client.ReceiveTimeout = 1000;
			udpClient.JoinMulticastGroup( mMulticastAddress );

			mContinueListening = true;

			Task.Factory.StartNew( () => {
				mLog.LogMessage( string.Format( "Multicast listener starting for {0}", mMulticastAddress ));

				while( mContinueListening ) {
					try {
						Byte[]	data = udpClient.Receive( ref endPoint );
						var		request = Encoding.UTF8.GetString( data );

						responder.OnRequest( UdpMessage.FromData( request, endPoint ));
					}
					catch( Exception ex ) {
						if((uint)ex.HResult != 0x80004005 ) {
							mLog.LogException( string.Format( "Multicast listener for {0}", mMulticastAddress ), ex );
						}
					}
				}

				mLog.LogMessage( string.Format( "Multicast listener exiting for {0}", mMulticastAddress ));
			} );
		}

		public void StopListener() {
			mContinueListening = false;
		}

		public void SendMulticast( string data ) {
			var udpclient = new UdpClient();
			var	endPoint = new IPEndPoint( mMulticastAddress, mMulticastPort );
			var buffer = Encoding.UTF8.GetBytes( data );

			udpclient.JoinMulticastGroup( mMulticastAddress );
			udpclient.Send( buffer, buffer.Length, endPoint );
		}
	}
}
