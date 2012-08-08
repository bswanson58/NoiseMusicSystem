using System;
using ReusableBits.Support;

namespace ReusableBits.Threading {
	public enum RecurringTaskState{
		Ready,
		NotReady,
		Completed,
		Paused,
		Running
	}

	public class RecurringTaskContext {
		private readonly RecurringTask			mTask;
		private RecurringTaskState				mTaskState;
		private DateTime						mNextExecutionTime;

		public RecurringTaskContext( RecurringTask task ) {
			mTask = task;
			mTaskState = RecurringTaskState.NotReady;
			mNextExecutionTime = TimeProvider.Now();
		}

		public RecurringTask Task {
			get{ return( mTask ); }
		}

		public RecurringTaskState TaskState {
			get{ return( mTaskState ); }
		}

		public DateTime NextExecutionTime {
			get{ return( mNextExecutionTime ); }
		}

		public void UpdateNextExecutionTime() {
			var nextExecutionTime = mTask.TaskSchedule.CalculateNextExecutionTime();

			if( nextExecutionTime.HasValue ) {
				mTaskState = RecurringTaskState.Ready;
				mNextExecutionTime = nextExecutionTime.Value;
			}
			else {
				mTaskState = RecurringTaskState.Completed;
			}
		}

		public RecurringTaskContext ExecuteTask() {
			var startTime = TimeProvider.Now();

			mTaskState = RecurringTaskState.Running;
			mTask.Execute();
			mTaskState = RecurringTaskState.NotReady;

			mTask.TaskSchedule.UpdateLastExecutionTime( startTime, TimeProvider.Now());

			return( this );
		}
	}
}
