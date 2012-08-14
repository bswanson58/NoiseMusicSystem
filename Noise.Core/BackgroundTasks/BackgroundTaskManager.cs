using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Noise.Core.Support;
using Noise.Infrastructure;
using ReusableBits.Threading;

namespace Noise.Core.BackgroundTasks {
	public class BackgroundTaskManager : IBackgroundTaskManager, IRequireInitialization,
										 IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted> {
		internal const string						cBackgroundTaskName		= "BackgroundTask";
		internal const string						cBackgroundTaskGroup	= "BackgroundTaskManager";

		private readonly IEventAggregator			mEventAggregator;
		private readonly IRecurringTaskScheduler	mJobScheduler;
		private IEnumerator<IBackgroundTask>		mTaskEnum;
		private bool								mRunningTaskFlag;
		private bool								mUpdateInProgress;

		private readonly IEnumerable<IBackgroundTask>	mBackgroundTasks;

		public BackgroundTaskManager( IEventAggregator eventAggregator, ILifecycleManager lifecycleManager,
									  IRecurringTaskScheduler recurringTaskScheduler, IEnumerable<IBackgroundTask> backgroundTasks ) {
			mBackgroundTasks = backgroundTasks;
			mEventAggregator = eventAggregator;
			mJobScheduler = recurringTaskScheduler;

			lifecycleManager.RegisterForInitialize( this );

			NoiseLogger.Current.LogInfo( "BackgroundTaskManager created" );
		}

		public void Initialize() {
			var backgroundJob = new RecurringTask( Execute, "Background Tasks" );

			backgroundJob.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									  .Delay( RecurringInterval.FromSeconds( 15 ));
			mJobScheduler.AddRecurringTask( backgroundJob );

			mTaskEnum = mBackgroundTasks.GetEnumerator();

			mEventAggregator.Subscribe( this );
		}

		public void Execute( RecurringTask job ) {
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
			mJobScheduler.RemoveAllTasks();
		}

		public void Handle( Events.LibraryUpdateStarted eventArgs ) {
			mUpdateInProgress = true;
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			mUpdateInProgress = false;
		}
	}
}
