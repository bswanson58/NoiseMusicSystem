using System;
using System.ServiceModel;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support.Service;
using Noise.Service.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceBus;
using Quartz;
using Quartz.Impl;

namespace Noise.ServiceImpl.LibraryUpdate {
	public class LibraryServiceImpl : BaseService {
		internal const string				cNoiseLibraryUpdate = "NoiseLibraryUpdate";

		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private INoiseManager				mNoiseManager;
		private	ISchedulerFactory			mSchedulerFactory;
		private	IScheduler					mJobScheduler;
		private IServiceBusManager			mServiceBus;
		private ServiceHost					mLibraryUpdateServiceHost;
		private readonly ILog				mLog;

		public LibraryServiceImpl( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();
		}

		public override void OnStart( string[] args ) {
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mContainer.RegisterInstance( mNoiseManager );

			if( mNoiseManager.Initialize()) {
				mSchedulerFactory = new StdSchedulerFactory();
				mJobScheduler = mSchedulerFactory.GetScheduler();
				mJobScheduler.Start();

//				ScheduleLibraryUpdate();
				// Always update the library on file system changes in the service.
				mNoiseManager.LibraryBuilder.EnableUpdateOnLibraryChange = true;

				mServiceBus = mContainer.Resolve<IServiceBusManager>();
				if( mServiceBus.InitializeServer()) {
					mEvents.GetEvent<Events.LibraryUpdateStarted>().Subscribe( OnLibraryUpdateStarted );
					mEvents.GetEvent<Events.LibraryUpdateCompleted>().Subscribe( OnLibraryUpdateCompleted );
				}

				try {
//					const string queueName = @".\private$\Noise.LibraryUpdate.Control";
//					if(!MessageQueue.Exists( queueName )) {
//						MessageQueue.Create( queueName, true );
//					}

					mLibraryUpdateServiceHost = new ServiceHost( new LibraryUpdateService( mContainer ));
					mLibraryUpdateServiceHost.Open();

 				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - LibraryUpdateService host start:", ex );
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

		private void ScheduleLibraryUpdate() {
			var jobDetail = new JobDetail( cNoiseLibraryUpdate, "LibraryUpdater", typeof( LibraryUpdateJob ));
			var trigger = new SimpleTrigger( cNoiseLibraryUpdate, "LibraryUpdater",
											 DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 15 )); 

			trigger.JobDataMap[cNoiseLibraryUpdate] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			mLog.LogMessage( "Started Library Update schedule." );
		}

		public void UpdateLibrary() {
			if(!mNoiseManager.LibraryBuilder.LibraryUpdateInProgress ) {
				mNoiseManager.LibraryBuilder.StartLibraryUpdate();
			}
		}

		private void OnLibraryUpdateStarted( long libraryId ) {
			mServiceBus.Publish( new LibraryUpdateStartedMessage { LibraryId = libraryId });
		}

		private void OnLibraryUpdateCompleted( long libraryId ) {
			mServiceBus.Publish( new LibraryUpdateCompletedMessage { LibraryId = libraryId });
		}
	}
}
