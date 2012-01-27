using System;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Service.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class ApplicationSupport : IHandle<Events.UrlLaunchRequest>, IHandle<Events.LaunchRequest> {
		private readonly IEventAggregator	mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IServiceBusManager	mServiceBus;
		private readonly HotkeyManager		mHotkeyManager;

		public ApplicationSupport( IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator, IServiceBusManager serviceBusManager ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mServiceBus = serviceBusManager;
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

			mEventAggregator.Subscribe( this );

			return( true );
		}

		public void Shutdown() {
		}

		public void Handle( Events.UrlLaunchRequest eventArgs ) {
			try {
				System.Diagnostics.Process.Start( eventArgs.Url );
			}
			catch( Exception ) {
				try {
					var startInfo = new System.Diagnostics.ProcessStartInfo( "IExplore.exe", eventArgs.Url );

					System.Diagnostics.Process.Start( startInfo );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - OnWebsiteRequested:", ex );
				}
			}
		}

		public void Handle( Events.LaunchRequest eventArgs ) {
			try {
				System.Diagnostics.Process.Start( eventArgs.Target );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - OnLaunchRequest:", ex );
			}
		}
	}
}
