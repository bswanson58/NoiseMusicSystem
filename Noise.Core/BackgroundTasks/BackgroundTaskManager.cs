﻿using System;
using System.Collections.Generic;
using Microsoft.Practices.Prism.Events;
using Noise.Core.Support;
using Noise.Infrastructure;
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

	public class BackgroundTaskManager : IBackgroundTaskManager, IRequireInitialization {
		internal const string					cBackgroundTaskName		= "BackgroundTask";
		internal const string					cBackgroundTaskGroup	= "BackgroundTaskManager";

		private readonly IEventAggregator		mEvents;
		private	readonly ISchedulerFactory		mSchedulerFactory;
		private	readonly IScheduler				mJobScheduler;
		private readonly JobDetail				mTaskJobDetail;
		private	readonly Trigger				mTaskExecuteTrigger;
		private IEnumerator<IBackgroundTask>	mTaskEnum;
		private bool							mRunningTaskFlag;
		private bool							mUpdateInProgress;

		private readonly IEnumerable<IBackgroundTask>	mBackgroundTasks;

		public BackgroundTaskManager( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager, IEnumerable<IBackgroundTask> backgroundTasks ) {
			mBackgroundTasks = backgroundTasks;
			mEvents = eventAggregator;

			lifecycleManager.RegisterForInitialize( this );

			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();

			mTaskJobDetail = new JobDetail( cBackgroundTaskName, cBackgroundTaskGroup, typeof( BackgroundTaskJob ));

			mTaskExecuteTrigger = new SimpleTrigger( cBackgroundTaskName, cBackgroundTaskGroup,
														DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null,
														SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 5 )); 
			mTaskExecuteTrigger.JobDataMap[cBackgroundTaskName] = this;

			NoiseLogger.Current.LogInfo( "BackgroundTaskManager created" );
		}

		public void Initialize() {
			mJobScheduler.Start();
			mTaskEnum = mBackgroundTasks.GetEnumerator();

			mJobScheduler.ScheduleJob( mTaskJobDetail, mTaskExecuteTrigger );

			mEvents.GetEvent<Events.LibraryUpdateStarted>().Subscribe( OnLibraryUpdateStarted );
			mEvents.GetEvent<Events.LibraryUpdateCompleted>().Subscribe( OnLibraryUpdateCompleted );
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

					NoiseLogger.Current.LogException( string.Format( "Exception - BackgroundTaskMgr '{0}'", taskId ), ex );
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

		public void Shutdown() {
			mJobScheduler.Shutdown( true );
		}

		private void OnLibraryUpdateStarted( long libraryId ) {
			mUpdateInProgress = true;
		}

		private void OnLibraryUpdateCompleted( long libraryId ) {
			mUpdateInProgress = false;
		}
	}
}
