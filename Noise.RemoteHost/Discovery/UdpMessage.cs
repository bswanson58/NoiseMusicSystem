using System.Diagnostics;
using System.Net;

namespace Noise.RemoteHost.Discovery {
	[DebuggerDisplay("Message = {AsData()}")]
	public class UdpMessage {
		public string		Realm {  get; private set; }
		public string		Command { get; private set; }
		public IPEndPoint	Address;
		public string		Message;

		public static UdpMessage FromData( string message, IPEndPoint address ) {
			var retValue = FromData( message );

			retValue.Address = address;

			return( retValue );
		}

		public static UdpMessage FromData( string message ) {
			var retValue = default( UdpMessage );

			if( !string.IsNullOrWhiteSpace( message )) {
				var messageParts = message.Split( new [] {'|' }, 3 );

				if( messageParts.Length == 3 ) {
					retValue = new UdpMessage( messageParts[0], messageParts[1], messageParts[3]);
				}
				else if( messageParts.Length == 2 ) {
					retValue =  new UdpMessage( messageParts[0], messageParts[1]);
				}
				else {
					retValue = new UdpMessage { Message = message };
				}
			}

			return( retValue );
		}

		private UdpMessage() {
			Realm = string.Empty;
			Command = string.Empty;
			Message = string.Empty;
		}

		public UdpMessage( string realm, string command ) :
			this() {
			Realm = realm;
			Command = command;
		}

		public UdpMessage( string realm, string command, string message ) :
			this() {
			Realm = realm;
			Command = command;
			Message = message;
		}

		public string AsData() {
			return( Realm + "|" + Command + "|" + Message );
		}
	}

	public interface IRequestResponder {
		void	OnRequest( UdpMessage message );
	}

	public interface IRequestSender {
		void	SendResponse( string address, UdpMessage message );
	}

}
