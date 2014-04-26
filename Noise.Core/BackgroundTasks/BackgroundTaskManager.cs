using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Threading;

namespace Noise.Core.BackgroundTasks {
	public class BackgroundTaskManager : IBackgroundTaskManager, IRequireConstruction,
										 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
										 IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted> {
		internal const string							cBackgroundTaskName		= "BackgroundTask";
		internal const string							cBackgroundTaskGroup	= "BackgroundTaskManager";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IRecurringTaskScheduler		mJobScheduler;
		private readonly IEnumerator<IBackgroundTask>	mTaskEnum;
		private bool									mRunningTaskFlag;
		private bool									mUpdateInProgress;

		private readonly IEnumerable<IBackgroundTask>	mBackgroundTasks;

		public BackgroundTaskManager( IEventAggregator eventAggregator,
									  IRecurringTaskScheduler recurringTaskScheduler, IEnumerable<IBackgroundTask> backgroundTasks ) {
			mBackgroundTasks = backgroundTasks;
			mEventAggregator = eventAggregator;
			mJobScheduler = recurringTaskScheduler;

			mTaskEnum = mBackgroundTasks.GetEnumerator();
			mEventAggregator.Subscribe( this );

			NoiseLogger.Current.LogInfo( "BackgroundTaskManager created" );
		}

		public void Handle( Events.DatabaseOpened args ) {
			var backgroundJob = new RecurringTask( Execute, cBackgroundTaskName );

			backgroundJob.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									  .Delay( RecurringInterval.FromSeconds( 15 ));
			mJobScheduler.AddRecurringTask( backgroundJob );
		}

		public void Handle( Events.DatabaseClosing args ) {
			mJobScheduler.RemoveTask( cBackgroundTaskName );
			mUpdateInProgress = false;
		}

		public void Handle( Events.LibraryUpdateStarted eventArgs ) {
			mUpdateInProgress = true;
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			mUpdateInProgress = false;
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
	}
}
