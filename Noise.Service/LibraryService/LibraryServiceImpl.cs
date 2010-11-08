using System;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Service.ServiceBus;
using Noise.Service.Support;
using Quartz;
using Quartz.Impl;

namespace Noise.Service.LibraryService {
	public class LibraryServiceImpl : BaseService {
		internal const string				cNoiseLibraryUpdate = "NoiseLibraryUpdate";

		private readonly IUnityContainer	mContainer;
		private INoiseManager				mNoiseManager;
		private	ISchedulerFactory			mSchedulerFactory;
		private	IScheduler					mJobScheduler;
		private MessagePublisher			mMessagePublisher;
		private readonly ILog				mLog;

		public LibraryServiceImpl( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public override void OnStart( string[] args ) {
//			mNoiseManager = mContainer.Resolve<INoiseManager>();
//			mContainer.RegisterInstance( mNoiseManager );

//			if( mNoiseManager.Initialize()) {
				mSchedulerFactory = new StdSchedulerFactory();
				mJobScheduler = mSchedulerFactory.GetScheduler();
				mJobScheduler.Start();

				ScheduleLibraryUpdate();
//				InitializeLibraryWatchers();

				mMessagePublisher = new MessagePublisher( mContainer );
				if( mMessagePublisher.InitializePublisher()) {
					mMessagePublisher.StartPublisher();
				}
//			}
		}

		public override void OnShutdown() {
//			mNoiseManager.Shutdown();
		}

		private void ScheduleLibraryUpdate() {
			var jobDetail = new JobDetail( cNoiseLibraryUpdate, "LibraryUpdater", typeof( LibraryUpdateJob ));
			var trigger = new SimpleTrigger( cNoiseLibraryUpdate, "LibraryUpdater",
											 DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 15 )); 

			trigger.JobDataMap[cNoiseLibraryUpdate] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			mLog.LogMessage( "Started Library Updates." );
		}

		private void InitializeLibraryWatchers() {
			var	rootFolderList = mNoiseManager.LibraryBuilder.RootFolderList();
			
		}

		public void UpdateLibrary() {
//			if(!mNoiseManager.LibraryBuilder.LibraryUpdateInProgress ) {
//				mNoiseManager.LibraryBuilder.StartLibraryUpdate();
//			}
			var events = mContainer.Resolve<IEventAggregator>();

			events.GetEvent<Events.LibraryUpdateStarted>().Publish( this );
		}
	}
}
