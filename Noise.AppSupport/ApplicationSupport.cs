using System;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using Noise.Service.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class ApplicationSupport {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IServiceBusManager	mServiceBus;
		private readonly IRemoteServer		mRemoteServer;
		private readonly ILog				mLog;
		private readonly HotkeyManager		mHotkeyManager;

		public ApplicationSupport( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
			mServiceBus = mContainer.Resolve<IServiceBusManager>();
			mRemoteServer = mContainer.Resolve<IRemoteServer>();
			mHotkeyManager = new HotkeyManager( mContainer );
		}

		public bool Initialize() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			if(( configuration != null ) &&
			   ( configuration.UseServer )) {
				if(!mServiceBus.Initialize( configuration.ServerName )) {
					mLog.LogMessage( "ServiceBusManager was not initialized." );
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

		private void OnWebsiteRequested( string url ) {
			try {
				System.Diagnostics.Process.Start( url );
			}
			catch( Exception ex1 ) {
				try {
					var startInfo = new System.Diagnostics.ProcessStartInfo( "IExplore.exe", url );

					System.Diagnostics.Process.Start( startInfo );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - OnWebsiteRequested:", ex );
				}
			}
		}

		private void OnLaunchRequest( string path ) {
			try {
				System.Diagnostics.Process.Start( path );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - OnLaunchRequest:", ex );
			}
		}
	}
}
