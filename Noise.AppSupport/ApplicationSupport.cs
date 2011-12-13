using System;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.RemoteHost;
using Noise.Service.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class ApplicationSupport {
		private readonly IEventAggregator	mEvents;
		private readonly IServiceBusManager	mServiceBus;
		private readonly IRemoteServer		mRemoteServer;
		private readonly HotkeyManager		mHotkeyManager;

		public ApplicationSupport( IEventAggregator eventAggregator, IServiceBusManager serviceBusManager, IRemoteServer remoteServer ) {
			mEvents = eventAggregator;
			mServiceBus = serviceBusManager;
			mRemoteServer = remoteServer;
			mHotkeyManager = new HotkeyManager( mEvents );
		}

		public bool Initialize() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			if(( configuration != null ) &&
			   ( configuration.UseServer )) {
				if(!mServiceBus.Initialize( configuration.ServerName )) {
					NoiseLogger.Current.LogMessage( "ServiceBusManager was not initialized." );
				}
			}

			mHotkeyManager.Initialize();

			mEvents.GetEvent<Events.WebsiteRequest>().Subscribe( OnWebsiteRequested );
			mEvents.GetEvent<Events.LaunchRequest>().Subscribe( OnLaunchRequest );

			mRemoteServer.OpenRemoteServer();

			return( true );
		}

		public void Shutdown() {
			mRemoteServer.CloseRemoteServer();
		}

		private static void OnWebsiteRequested( string url ) {
			try {
				System.Diagnostics.Process.Start( url );
			}
			catch( Exception ) {
				try {
					var startInfo = new System.Diagnostics.ProcessStartInfo( "IExplore.exe", url );

					System.Diagnostics.Process.Start( startInfo );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - OnWebsiteRequested:", ex );
				}
			}
		}

		private static void OnLaunchRequest( string path ) {
			try {
				System.Diagnostics.Process.Start( path );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - OnLaunchRequest:", ex );
			}
		}
	}
}
