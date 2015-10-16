namespace Noise.RemoteHost.Discovery {
	public class RequestSender : IRequestSender {
		private readonly int	mPort;

		public RequestSender( int port ) {
			mPort = port;
		}

		public void SendResponse( string address, UdpMessage message ) {
			var sender = UdpSender.ConnectTo( address, mPort );

			sender.Send( message );
		}
	}
}
