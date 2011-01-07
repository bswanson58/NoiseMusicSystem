using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Quartz;
using Quartz.Impl;

namespace Noise.Core.BackgroundTasks {
	internal class BackgroundTaskJob : IJob {
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
		private readonly ILog					mLog;
		private	readonly ISchedulerFactory		mSchedulerFactory;
		private	readonly IScheduler				mJobScheduler;
		private IEnumerator<IBackgroundTask>	mTaskEnum;

		[ImportMany( typeof( IBackgroundTask ))]
		public IEnumerable<IBackgroundTask>	BackgroundTasks;

		public BackgroundTaskManager( IUnityContainer container ) {
			mContainer = container;

			mLog = mContainer.Resolve<ILog>();
						
			mSchedulerFactory = new StdSchedulerFactory();
			mJobScheduler = mSchedulerFactory.GetScheduler();
			mJobScheduler.Start();
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

			ScheduleTaskJob();

			return( true );
		}

		private void ScheduleTaskJob() {
			var jobDetail = new JobDetail( cBackgroundTaskName, cBackgroundTaskGroup, typeof( BackgroundTaskJob ));
			var trigger = new SimpleTrigger( cBackgroundTaskName, cBackgroundTaskGroup,
											 DateTime.UtcNow + TimeSpan.FromSeconds( 10 ), null, SimpleTrigger.RepeatIndefinitely, TimeSpan.FromSeconds( 5 )); 

			trigger.JobDataMap[cBackgroundTaskName] = this;

			mJobScheduler.ScheduleJob( jobDetail, trigger );
		}

		public void Execute() {
			IBackgroundTask	task = null;

			try {
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
	}
}
