using System;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Noise.Service.LibraryService {
	public class LibraryServiceImpl : IWindowsService {
		internal const string				cNoiseLibraryUpdate = "NoiseLibraryUpdate";

		private readonly IUnityContainer	mContainer;
		private INoiseManager				mNoiseManager;
		private	ISchedulerFactory			mSchedulerFactory;
		private	IScheduler					mJobScheduler;
		private readonly ILog				mLog;

		public LibraryServiceImpl( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void OnStart( string[] args ) {
			mNoiseManager = mContainer.Resolve<INoiseManager>();
			mContainer.RegisterInstance( mNoiseManager );

			if( mNoiseManager.Initialize()) {
				mSchedulerFactory = new StdSchedulerFactory();
				mJobScheduler = mSchedulerFactory.GetScheduler();
				mJobScheduler.Start();

				ScheduleLibraryUpdate();
				InitializeLibraryWatchers();
			}
		}

		public void OnStop() {
		}

		public void OnPause() {
		}

		public void OnContinue() {
		}

		public void OnShutdown() {
			mNoiseManager.Shutdown();
		}

		public void Dispose() {
		}

		private void ScheduleLibraryUpdate() {
			var jobDetail = new JobDetail( cNoiseLibraryUpdate, "LibraryUpdater", typeof( LibraryUpdateJob ));
			var trigger = new SimpleTrigger( cNoiseLibraryUpdate, "LibraryUpdater",
											 DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromMinutes( 30 )); 

			trigger.JobDataMap[cNoiseLibraryUpdate] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
			mLog.LogMessage( "Started Library Updates." );
		}

		private void InitializeLibraryWatchers() {
			var	rootFolderList = mNoiseManager.LibraryBuilder.RootFolderList();
			
		}

		public void UpdateLibrary() {
			if(!mNoiseManager.LibraryBuilder.LibraryUpdateInProgress ) {
				mNoiseManager.LibraryBuilder.StartLibraryUpdate();
			}
		}
	}
}
