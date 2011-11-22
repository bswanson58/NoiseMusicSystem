using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Bonjour;

namespace Noise.RemoteHost {
	//
	// ServiceData
	//
	// Holds onto the information associated with a located service on the network
	//
	public class ServiceData {
		public uint			InterfaceIndex { get; set; }
		public string		Name { get; set; }
		public string		HostName { get; set; }
		public string		Type { get; set; }
		public string		Domain { get; set; }
		public IPAddress	Address { get; set; }
		public ushort		Port { get; set; }

		public string FullName {
			get { return( Name + "." + Type + Domain ); }
		}

		public override string ToString() {
			return Name;
		}
	};

	public class ServiceDiscovery {
		private DNSSDService            mService;
		private DNSSDService            mRegistrar;
		private DNSSDService            mServiceBrowser;
		private DNSSDEventManager		mEventManager;
		private string					mRegisteredName;
		private List<ServiceData>			mLocatedServices;

		public	delegate void			ServiceLocated( string registrationName, string address, ushort port );
		public	event ServiceLocated	OnServiceLocated;

		public bool Initialize() {
			var retValue = true;

			try {
				mLocatedServices = new List<ServiceData>();

				mService = new DNSSDService();
				mEventManager = new DNSSDEventManager();

				mEventManager.ServiceRegistered += ServiceRegistered;
				mEventManager.ServiceFound += ServiceFound;
				mEventManager.ServiceLost += ServiceLost;
				mEventManager.ServiceResolved += ServiceResolved;
				mEventManager.QueryRecordAnswered += QueryAnswered;
				mEventManager.OperationFailed += OperationFailed;
			}
			catch {
				retValue = false;
			}

			return ( retValue );
		}

		public void RegisterService( string registrationType, string registrationName, ushort port ) {
			mRegistrar = mService.Register( 0, 0, registrationName, registrationType, null, null, port, null, mEventManager );
		}

		// ServiceRegistered
		//
		// Called by DNSServices core as a result of Register() call
		//
		private void ServiceRegistered( DNSSDService service, DNSSDFlags flags, String name, String regType, String domain ) {
			mRegisteredName = name;
		}

		public void UnregisterService() {
			if( mRegistrar != null ) {
				mRegistrar.Stop();
			}
		}

		public void LocateServices( string registrationType ) {
			if( mServiceBrowser != null ) {
				mServiceBrowser.Stop();

				mServiceBrowser = null;
			}

			try {
				mServiceBrowser = mService.Browse( 0, 0, registrationType, null, mEventManager );
			}
			catch {
				mServiceBrowser = null;
			}
		}

		//
		// ServiceFound
		//
		// Called by DNSServices core as a result of a Browse call
		//
		private void ServiceFound( DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain ) {
			if( serviceName != mRegisteredName ) {
				var service = new ServiceData { Name = serviceName, InterfaceIndex = ifIndex, Type = regType, Domain = domain };

				mLocatedServices.Add( service );
				mService.Resolve( 0, ifIndex, serviceName, regType, domain, mEventManager );
			}
		}

		//
		// ServiceLost
		//
		// Called by DNSServices core as a result of a Browse call
		//
		private void ServiceLost( DNSSDService sref, DNSSDFlags flags, uint ifIndex, String serviceName, String regType, String domain ) {
			var service = ( from serv in mLocatedServices where serv.Name == serviceName select serv ).FirstOrDefault();
			if( service != null ) {
				mLocatedServices.Remove( service );
			}
		}

		//
		// ServiceResolved
		//
		// Called by DNSServices core as a result of DNSService.Resolve()
		//
		private void ServiceResolved( DNSSDService resolver, DNSSDFlags flags, uint ifIndex, String fullName, String hostName, ushort port, TXTRecord txtRecord ) {
			if( mServiceBrowser != null ) {
				mServiceBrowser.Stop();

				mServiceBrowser = null;
			}

			resolver.Stop();

			var service = ( from serv in mLocatedServices 
							where ( serv.FullName == fullName ) && ( serv.InterfaceIndex == ifIndex ) select serv ).FirstOrDefault();
			if( service != null ) {
				service.HostName = hostName;
				service.InterfaceIndex = ifIndex;
				service.Port = port;
			}

			// Query for the IP address associated with "hostName"
			try {
				mService.QueryRecord( 0, ifIndex, hostName, DNSSDRRType.kDNSSDType_A, DNSSDRRClass.kDNSSDClass_IN, mEventManager );
			}
			catch {
				resolver.Stop();
			}
		}

		//
		// QueryAnswered
		//
		// Called by DNSServices core as a result of DNSService.QueryRecord()
		//
		private void QueryAnswered( DNSSDService resolver, DNSSDFlags flags, uint ifIndex, String hostName, DNSSDRRType rrtype, DNSSDRRClass rrclass, Object rdata, uint ttl ) {
			//
			// Stop the resolve to reduce the burden on the network
			//
			resolver.Stop();

			uint	bits = BitConverter.ToUInt32( (Byte[])rdata, 0 );
			var		address = new IPAddress( bits );

			var service = ( from serv in mLocatedServices where serv.HostName == hostName select serv ).FirstOrDefault();
			if( service != null ) {
				service.Address = address;

				if( OnServiceLocated != null ) {
					OnServiceLocated( service.Name, service.Address.ToString(), service.Port );
				}
			}
		}

		public void OperationFailed( DNSSDService service, DNSSDError error ) {
			//            MessageBox.Show("Operation returned an error code " + error, "Error");
		}
	}
}
