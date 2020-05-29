using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using ReusableBits.Interfaces;
using ReusableBits.Support;

namespace ReusableBits.Threading {
	public class DefaultTaskScheduler : RecurringTaskScheduler {
		public DefaultTaskScheduler() { }
    }

	public class RecurringTaskScheduler : IRecurringTaskScheduler {
		private readonly List<RecurringTaskContext>			mTaskList;
		private readonly Timer								mTimer;
		private readonly TaskHandler<RecurringTaskContext>	mTaskHandler; 

		public RecurringTaskScheduler( TaskScheduler taskScheduler, TaskScheduler uiTaskScheduler ) {
			mTaskList = new List<RecurringTaskContext>();
			mTimer = new Timer( OnTimer, null, Timeout.Infinite, Timeout.Infinite );
			mTaskHandler = new TaskHandler<RecurringTaskContext>( taskScheduler, uiTaskScheduler );
		}

		public RecurringTaskScheduler() :
			this( TaskScheduler.Default, TaskScheduler.FromCurrentSynchronizationContext()) { }

		public void AddRecurringTask( RecurringTask task ) {
			Condition.Requires( task ).IsNotNull();

			var taskContext = new RecurringTaskContext( task );

			taskContext.UpdateNextExecutionTime();
			mTaskList.Add( taskContext );

			UpdateTimer();
		}

		public RecurringTask RetrieveTask( string taskName ) {
			return(( from t in mTaskList where t.Task.TaskId == taskName select t.Task ).FirstOrDefault());
		}

		public void RemoveAllTasks() {
			lock( mTaskList ) {
				mTaskList.Clear();
			}

			UpdateTimer();
		}

		public void RemoveTask( string taskName ) {
			lock( mTaskList ) {
				var removeList = from t in mTaskList where t.Task.TaskId == taskName select t;
				mTaskList.RemoveAll( removeList.Contains );
			}

			UpdateTimer();
		}

		protected void OnTimer( object sender ) {
			SetTimer( Timeout.Infinite );

			lock( mTaskList ) {
				var completedList = from t in mTaskList where t.TaskState == RecurringTaskState.Completed select t;
				mTaskList.RemoveAll( completedList.Contains);

				var readyList = from t in mTaskList where t.TaskState == RecurringTaskState.Ready orderby t.NextExecutionTime select t;
				var startTime = TimeProvider.Now();

				foreach( var task in readyList ) {
					if( task.NextExecutionTime <= startTime ) {
						mTaskHandler.StartTask( task.ExecuteTask, OnExecuteComplete, OnExecuteFault );
					}
				}
			}

			UpdateTimer();
		}

		private void OnExecuteComplete( RecurringTaskContext taskContext ) {
			taskContext.UpdateNextExecutionTime();

			UpdateTimer();
		}

		private void OnExecuteFault( Exception ex ) {
			
		}

		private void UpdateTimer() {
			RecurringTaskContext	nextTask;

			lock( mTaskList ) {
				nextTask = ( from t in mTaskList where t.TaskState == RecurringTaskState.Ready orderby t.NextExecutionTime select t ).FirstOrDefault();
			}

			if( nextTask != null ) {
				var nextTaskTime = nextTask.NextExecutionTime - TimeProvider.Now();
				var nextTime = Math.Max( 10, (int)nextTaskTime.TotalMilliseconds );

				SetTimer( nextTime );
			}
		}

		protected virtual void SetTimer( long nextTime ) {
			mTimer.Change( nextTime, Timeout.Infinite );
		}
	}
}
