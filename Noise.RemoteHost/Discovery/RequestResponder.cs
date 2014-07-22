using System;
using System.Collections.Generic;

namespace Noise.RemoteHost.Discovery {
	public class RequestResponder : IRequestResponder {
		private readonly string							mRealm;
		private readonly IRequestSender					mSender;
		private readonly Dictionary<string, UdpMessage>	mResponses;

		public RequestResponder( string realm, IRequestSender sender ) {
			mRealm = realm;
			mSender = sender;

			mResponses = new Dictionary<string, UdpMessage>();
		}

		public void AddRequestResponse( string request, string command, string response ) {
			mResponses.Add( request, new UdpMessage( mRealm, command, response ));
		}

		public void OnRequest( UdpMessage message ) {
			if( message.Realm.Equals( mRealm, StringComparison.OrdinalIgnoreCase )) {
				if( mResponses.ContainsKey( message.Command )) {
					mSender.SendResponse( message.Address.Address.ToString(), mResponses[message.Command]);
				}
			}
		}
	}
}
