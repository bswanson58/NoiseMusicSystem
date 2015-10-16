using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Noise.RemoteHost.Discovery {
	public abstract class UdpBase {
		protected UdpClient Client;

		protected UdpBase() {
			Client = new UdpClient();
		}

		public async Task<UdpMessage> Receive() {
			var result = await Client.ReceiveAsync();
			var retValue = UdpMessage.FromData( Encoding.UTF8.GetString( result.Buffer, 0, result.Buffer.Length ));

			retValue.Address = result.RemoteEndPoint;

			return( retValue );
		}
	}

	public class UdpListener : UdpBase {
		public UdpListener( int port )
			: this( new IPEndPoint( IPAddress.Any, port )) {
		}

		public UdpListener( IPEndPoint endpoint ) {
			Client = new UdpClient( endpoint );
		}

		public void Reply( string message, IPEndPoint endpoint ) {
			var datagram = Encoding.UTF8.GetBytes( message );

			Client.Send( datagram, datagram.Length, endpoint );
		}

	}

	public class UdpSender : UdpBase {
		private UdpSender() { }

		public static UdpSender ConnectTo( string hostname, int port ) {
			var connection = new UdpSender();

			connection.Client.Connect( hostname, port );
	
			return( connection );
		}

		public void Send( string message ) {
			var datagram = Encoding.UTF8.GetBytes( message );

			Client.Send( datagram, datagram.Length );
		}

		public void Send( UdpMessage message ) {
			Send( message.AsData());
		}

	}
}
