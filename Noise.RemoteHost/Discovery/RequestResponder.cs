using System;
using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.RemoteHost.Discovery {
	public class RequestResponder : IRequestResponder {
		private readonly string							mRealm;
		private readonly IRequestSender					mSender;
		private readonly Dictionary<string, UdpMessage>	mResponses;
		private readonly INoiseLog						mLog;

		public RequestResponder( string realm, IRequestSender sender, INoiseLog log ) {
			mRealm = realm;
			mSender = sender;
			mLog = log;

			mResponses = new Dictionary<string, UdpMessage>();
		}

		public void AddRequestResponse( string request, string command, string response ) {
			mResponses.Add( request, new UdpMessage( mRealm, command, response ));
		}

		public void OnRequest( UdpMessage message ) {
			mLog.LogMessage( string.Format( "UDP message received - Realm: {0}, Command: {1}, From: {2}", message.Realm, message.Command, message.Address.Address ));

			if( message.Realm.Equals( mRealm, StringComparison.OrdinalIgnoreCase )) {
				if( mResponses.ContainsKey( message.Command )) {
					mSender.SendResponse( message.Address.Address.ToString(), mResponses[message.Command]);

					mLog.LogMessage( string.Format( "UDP message response: {0}", mResponses[message.Command]));
				}
			}
		}
	}
}
