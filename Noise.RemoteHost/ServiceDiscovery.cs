using System;
using System.Net;
using Bonjour;

namespace Noise.RemoteHost {
	//
	// PeerData
	//
	// Holds onto the information associated with a peer on the network
	//
	public class PeerData {
		public uint InterfaceIndex;
		public String Name;
		public String Type;
		public String Domain;
		public IPAddress Address;
		public int Port;

		public override String
		ToString() {
			return Name;
		}

		public override bool
		Equals( object other ) {
			bool result = false;

			if( other != null ) {
				if( this == other ) {
					result = true;
				}
				else if( other is PeerData ) {
					PeerData otherPeerData = (PeerData)other;

					result = ( Name == otherPeerData.Name );
				}
			}

			return result;
		}


		public override int
		GetHashCode() {
			return Name.GetHashCode();
		}
	};

	public class ServiceDiscovery {
		private DNSSDService            m_service;
		private DNSSDService            m_registrar;
		private DNSSDService            m_browser;
		private DNSSDService            m_resolver;
		private DNSSDEventManager		mEventManager;
		private string					m_name;
		//        private Socket                  m_socket;

		public bool Initialize() {
			var retValue = true;

			try {
				m_service = new DNSSDService();
				mEventManager = new DNSSDEventManager();

				mEventManager.ServiceRegistered += ServiceRegistered;
				mEventManager.ServiceFound += ServiceFound;
				mEventManager.ServiceLost += ServiceLost;
				mEventManager.ServiceResolved += ServiceResolved;
				mEventManager.QueryRecordAnswered += QueryAnswered;
				mEventManager.OperationFailed += OperationFailed;

				//
				// start the register and browse operations
				//
				m_registrar = m_service.Register( 0, 0, "Noise.Desktop", "_Noise._tcp.", null, null, 88, null, mEventManager );
			}
			catch( Exception ex ) {
				retValue = false;
			}

			return ( retValue );
		}

		// ServiceRegistered
		//
		// Called by DNSServices core as a result of Register()
		// call
		//
		public void ServiceRegistered( DNSSDService service, DNSSDFlags flags, String name, String regType, String domain ) {
			m_name = name;

			//
			// Try to start browsing for other instances of this service
			//
			try {
				m_browser = m_service.Browse( 0, 0, "_Noisie._tcp.", null, mEventManager );
			}
			catch {
			}
		}

		//
		// ServiceFound
		//
		// Called by DNSServices core as a result of a Browse call
		//
		public void ServiceFound( DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain ) {
			if( serviceName != m_name ) {
				PeerData peer = new PeerData();

				peer.InterfaceIndex = ifIndex;
				peer.Name = serviceName;
				peer.Type = regType;
				peer.Domain = domain;
				peer.Address = null;

				m_resolver = m_service.Resolve( 0, peer.InterfaceIndex, peer.Name, peer.Type, peer.Domain, mEventManager );
			}
		}

		//
		// ServiceLost
		//
		// Called by DNSServices core as a result of a Browse call
		//
		public void ServiceLost( DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain ) {
			PeerData peer = new PeerData();

			peer.InterfaceIndex = ifIndex;
			peer.Name = serviceName;
			peer.Type = regType;
			peer.Domain = domain;
			peer.Address = null;

			//            comboBox1.Items.Remove(peer);
		}

		//
		// ServiceResolved
		//
		// Called by DNSServices core as a result of DNSService.Resolve()
		// call
		//
		public void ServiceResolved( DNSSDService sref, DNSSDFlags flags, uint ifIndex, String fullName, String hostName, ushort port, TXTRecord txtRecord ) {
			m_resolver.Stop();
			m_resolver = null;

			//            PeerData peer = (PeerData)comboBox1.SelectedItem;

			//            peer.Port = port;

			//
			// Query for the IP address associated with "hostName"
			//
			try {
				m_resolver = m_service.QueryRecord( 0, ifIndex, hostName, DNSSDRRType.kDNSSDType_A, DNSSDRRClass.kDNSSDClass_IN, mEventManager );
			}
			catch {
				//                MessageBox.Show("QueryRecord Failed", "Error");
				//                Application.Exit();
			}
		}

		//
		// QueryAnswered
		//
		// Called by DNSServices core as a result of DNSService.QueryRecord()
		// call
		//
		public void QueryAnswered( DNSSDService service, DNSSDFlags flags, uint ifIndex, String fullName, DNSSDRRType rrtype, DNSSDRRClass rrclass, Object rdata, uint ttl ) {
			//
			// Stop the resolve to reduce the burden on the network
			//
			m_resolver.Stop();
			m_resolver = null;

			//            PeerData peer = (PeerData) comboBox1.SelectedItem;
			uint bits = BitConverter.ToUInt32( (Byte[])rdata, 0 );
			IPAddress address = new IPAddress( bits );

			//            peer.Address = address;
		}

		public void OperationFailed( DNSSDService service, DNSSDError error ) {
			//            MessageBox.Show("Operation returned an error code " + error, "Error");
		}
	}
}
