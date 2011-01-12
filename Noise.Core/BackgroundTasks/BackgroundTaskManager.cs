using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Noise.Core.BackgroundTasks {
	internal class BackgroundTaskJob : IStatefulJob {
		public void Execute( JobExecutionContext context ) {
			if( context != null ) {
				var	manager = context.Trigger.JobDataMap[BackgroundTaskManager.cBackgroundTaskName] as BackgroundTaskManager;

				if( manager != null ) {
					manager.Execute();
				}
			}
		}
	}

	public class BackgroundTaskManager : IBackgroundTaskManager {
		internal const string					cBackgroundTaskName		= "BackgroundTask";
		internal const string					cBackgroundTaskGroup	= "BackgroundTaskManager";

		private	readonly IUnityContainer		mContainer;
		private readonly IEventAggregator		mEvents;
		private readonly ILog					mLog;
		private	readonly ISchedulerFactory		mSchedulerFactory;
		private	readonly IScheduler				mJobScheduler;
		private readonly JobDetail				mTaskJobDetail;
		private	readonly Trigger				mTaskExecuteTrigger;
		private IEnumerator<IBackgroundTask>	mTaskEnum;
		private bool							mRunningTaskFlag;
		private bool							mUpdateInProgress;

		[ImportMany( typeof( IBackgroundTask ))]
		public IEnumerable<IBackgroundTask>	BackgroundTasks;

		public BackgroundTaskManager( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();
			mLog = mContainer.Resolve<ILog>();

			mEvents.GetEvent<Events.LibraryUpdateStarted>().Subscribe( OnLibraryUpdateStarted );
			mEvents.GetEvent<Events.LibraryUpdateCompleted>().Subscribe( OnLibraryUpdateCompleted );
						
			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();
			mJobScheduler.Start();

			mTaskJobDetail = new JobDetail( cBackgroundTaskName, cBackgroundTaskGroup, typeof( BackgroundTaskJob ));

			mTaskExecuteTrigger = new SimpleTrigger( cBackgroundTaskName, cBackgroundTaskGroup,
														DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null,
														SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 5 )); 
			mTaskExecuteTrigger.JobDataMap[cBackgroundTaskName] = this;
		}

		public bool Initialize() {
			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );
			container.ComposeParts( this );

			foreach( var task in BackgroundTasks ) {
				if(!task.Initialize( mContainer )) {
					mLog.LogMessage( "BackgroundTaskManager could not initialize task '{0}'", task.TaskId );
				}
			}

			mTaskEnum = BackgroundTasks.GetEnumerator();

			mJobScheduler.ScheduleJob( mTaskJobDetail, mTaskExecuteTrigger );

			return( true );
		}

		public void Execute() {
			IBackgroundTask	task = null;

			if((!mUpdateInProgress ) &&
			   (!mRunningTaskFlag )) {
				try {
					mRunningTaskFlag = true;

					task = NextTask();

					if( task != null ) {
						task.ExecuteTask();
					}
				}
				catch( Exception ex ) {
					var taskId = "null";

					if( task != null ) {
						taskId = task.TaskId;
					}

					mLog.LogException( string.Format( "Exception - BackgroundTaskMgr '{0}'", taskId ), ex );
				}
				finally {
					mRunningTaskFlag = false;
				}
			}
		}

		private IBackgroundTask NextTask() {
			if(!mTaskEnum.MoveNext()) {
				mTaskEnum.Reset();

				mTaskEnum.MoveNext();
			}

			return( mTaskEnum.Current );
		}

		public void Stop() {
			mJobScheduler.Shutdown( true );

			foreach( var task in BackgroundTasks ) {
				task.Shutdown();
			}
		}

		private void OnLibraryUpdateStarted( long libraryId ) {
			mUpdateInProgress = true;
		}

		private void OnLibraryUpdateCompleted( long libraryId ) {
			mUpdateInProgress = false;
		}
	}
}
