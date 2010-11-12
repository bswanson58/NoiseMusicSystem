using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class ApplicationSupport {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private readonly IServiceBusManager	mServiceBus;
		private readonly ILog				mLog;

		public ApplicationSupport( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();

			mServiceBus = mContainer.Resolve<IServiceBusManager>();

			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ServerConfiguration>( ServerConfiguration.SectionName );

			if(( configuration != null ) &&
			   ( configuration.UseServer )) {
				if(!mServiceBus.Initialize( configuration.ServerName )) {
					mLog.LogMessage( "ServiceBusManager was not initialized." );
				}
			}

			mEvents.GetEvent<Events.WebsiteRequest>().Subscribe( OnWebsiteRequested );
		}

		private static void OnWebsiteRequested( string url ) {
			System.Diagnostics.Process.Start( url );
		}
	}
}
