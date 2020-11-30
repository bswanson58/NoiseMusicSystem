using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Interfaces;
using ReusableBits.Threading;

namespace Noise.Core.BackgroundTasks {
	public class BackgroundTaskManager : IBackgroundTaskManager, IRequireInitialization,
										 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
										 IHandle<Events.LibraryUpdateStarted>, IHandle<Events.LibraryUpdateCompleted> {
		internal const string							cBackgroundTaskName		= "BackgroundTask";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IRecurringTaskScheduler		mJobScheduler;
		private readonly IEnumerator<IBackgroundTask>	mTaskEnum;
		private readonly ILogBackgroundTasks			mLog;
		private bool									mRunningTaskFlag;

		private readonly IEnumerable<IBackgroundTask>	mBackgroundTasks;

		public BackgroundTaskManager( IEventAggregator eventAggregator, IRecurringTaskScheduler recurringTaskScheduler, ILogBackgroundTasks log,
                                      IBackgroundTask[] backgroundTasks, ILifecycleManager lifecycleManager ) {
			mBackgroundTasks = backgroundTasks;
			mEventAggregator = eventAggregator;
			mJobScheduler = recurringTaskScheduler;
			mLog = log;
			mTaskEnum = mBackgroundTasks.GetEnumerator();

			lifecycleManager.RegisterForInitialize( this );
			lifecycleManager.RegisterForShutdown( this );
		}

		public void Initialize() {
            mEventAggregator.Subscribe( this );
        }

		public void Shutdown() {
            mEventAggregator.Unsubscribe( this );

			StopTasks();
        }

		public void Handle( Events.DatabaseOpened args ) {
			StartTasks();
		}

		public void Handle( Events.DatabaseClosing args ) {
			StopTasks();
		}

		public void Handle( Events.LibraryUpdateStarted eventArgs ) {
			StopTasks();
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			StartTasks();
		}

		private void StartTasks() {
			// Stop any existing task.
			mJobScheduler.RemoveTask( cBackgroundTaskName );

			var backgroundJob = new RecurringTask( Execute, cBackgroundTaskName );

			backgroundJob.TaskSchedule.StartAt( RecurringInterval.FromSeconds( 15 ))
									  .Delay( RecurringInterval.FromSeconds( 15 ));
			mJobScheduler.AddRecurringTask( backgroundJob );

			mLog.LogTasksStarting( mBackgroundTasks.Count());
		}

		private void StopTasks() {
			mJobScheduler.RemoveTask( cBackgroundTaskName );

			mLog.LogTasksStopping();
		}

		private void Execute( RecurringTask job ) {
			IBackgroundTask	task = null;

			if(!mRunningTaskFlag ) {
				try {
					mRunningTaskFlag = true;

					task = NextTask();

                    task?.ExecuteTask();
                }
				catch( Exception ex ) {
					var taskId = "null";

					if( task != null ) {
						taskId = task.TaskId;
					}

					mLog.LogException( $"Executing background task '{taskId}'", ex );
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
