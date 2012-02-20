using System;
using System.ServiceModel;
using System.ServiceProcess;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support.Service;
using Noise.Service.Infrastructure.Clients;
using Noise.Service.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceBus;
using Quartz;
using Quartz.Impl;

namespace Noise.ServiceImpl.LibraryUpdate {
	[WindowsService("Noise Library Update Service",
		DisplayName = "Noise Library Update Service",
		Description = "This service maintains the Noise Music System library.",
		EventLogSource = "Noise Update Service",
		StartMode = ServiceStartMode.Automatic)]
	public class LibraryServiceImpl : BaseService, IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted> {
		internal const string				cNoiseLibraryUpdate = "NoiseLibraryUpdate";

		private readonly IEventAggregator	mEventAggregator;
		private readonly INoiseManager		mNoiseManager;
		private readonly ILibraryBuilder	mLibraryBuilder;
		private readonly IServiceBusManager	mServiceBus;
		private	ISchedulerFactory			mSchedulerFactory;
		private	IScheduler					mJobScheduler;
		private ServiceHost					mLibraryUpdateServiceHost;

		public LibraryServiceImpl( IEventAggregator eventAggregator, INoiseManager noiseManager,
								   ILibraryBuilder libraryBuilder, IServiceBusManager serviceBusManager ) {
			mEventAggregator = eventAggregator;
			mNoiseManager = noiseManager;
			mLibraryBuilder = libraryBuilder;
			mServiceBus = serviceBusManager;
		}

		public override void OnStart( string[] args ) {

			if( mNoiseManager.Initialize()) {
				mSchedulerFactory = new StdSchedulerFactory();
				mJobScheduler = mSchedulerFactory.GetScheduler();
				mJobScheduler.Start();

//				ScheduleLibraryUpdate();
				// Always update the library on file system changes in the service.
				mLibraryBuilder.EnableUpdateOnLibraryChange = true;

				if( mServiceBus.InitializeServer()) {
					mEventAggregator.Subscribe( this );
				}

				try {
//					const string queueName = @".\private$\Noise.LibraryUpdate.Control";
//					if(!MessageQueue.Exists( queueName )) {
//						MessageQueue.Create( queueName, true );
//					}

					mLibraryUpdateServiceHost = new ServiceHost( new LibraryUpdateService( mLibraryBuilder ));
					mLibraryUpdateServiceHost.Open();

 				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LibraryUpdateService host start:", ex );
				}
			}
		}

		public override void OnShutdown() {
			if(( mLibraryUpdateServiceHost != null ) &&
			   ( mLibraryUpdateServiceHost.State == CommunicationState.Opened )) {
				mLibraryUpdateServiceHost.Close();
			}

			mNoiseManager.Shutdown();
		}

/*		private void ScheduleLibraryUpdate() {
			var jobDetail = new JobDetail( cNoiseLibraryUpdate, "LibraryUpdater", typeof( LibraryUpdateJob ));
			var trigger = new SimpleTrigger( cNoiseLibraryUpdate, "LibraryUpdater",
											 DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 15 )); 

			trigger.JobDataMap[cNoiseLibraryUpdate] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			NoiseLogger.Current.LogMessage( "Started Library Update schedule." );
		}
*/
		public void UpdateLibrary() {
			if(!mLibraryBuilder.LibraryUpdateInProgress ) {
				mLibraryBuilder.StartLibraryUpdate();
			}
		}

		public void Handle( Events.LibraryUpdateStarted eventArgs ) {
			mServiceBus.Publish( new LibraryUpdateStartedMessage { LibraryId = eventArgs.LibraryId });
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			mServiceBus.Publish( new LibraryUpdateCompletedMessage { LibraryId = eventArgs.LibraryId });
		}
	}
}
