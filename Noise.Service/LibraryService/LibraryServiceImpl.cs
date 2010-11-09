using System;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceBus;
using Noise.Service.Support;
using Quartz;
using Quartz.Impl;

namespace Noise.Service.LibraryService {
	public class LibraryServiceImpl : BaseService {
		internal const string				cNoiseLibraryUpdate = "NoiseLibraryUpdate";

		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private INoiseManager				mNoiseManager;
		private	ISchedulerFactory			mSchedulerFactory;
		private	IScheduler					mJobScheduler;
		private IServiceBusManager			mServiceBus;
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

				ScheduleLibraryUpdate();
				InitializeLibraryWatchers();

				mServiceBus = mContainer.Resolve<IServiceBusManager>();
				if( mServiceBus.InitializeServer()) {
					mEvents.GetEvent<Events.LibraryUpdateStarted>().Subscribe( OnLibraryUpdateStarted );
					mEvents.GetEvent<Events.LibraryUpdateCompleted>().Subscribe( OnLibraryUpdateCompleted );
				}
			}
		}

		public override void OnShutdown() {
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

		private void InitializeLibraryWatchers() {
			var	rootFolderList = mNoiseManager.LibraryBuilder.RootFolderList();
			
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
