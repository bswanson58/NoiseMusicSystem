using System;
using CuttingEdge.Conditions;

namespace ReusableBits.Threading {
	public class RecurringTask {
		private readonly string					mTaskId;
		private readonly Action<RecurringTask>	mAction;
		private readonly RecurringTaskSchedule	mTaskSchedule;

		public RecurringTask( Action<RecurringTask> action ) :
			this( action, Guid.NewGuid().ToString()) { }

		public RecurringTask( Action<RecurringTask> action, string taskId ) {
			Condition.Requires( action ).IsNotNull();

			mAction = action;
			mTaskId = taskId;
			mTaskSchedule = new RecurringTaskSchedule();
		}

		public string TaskId {
			get{ return( mTaskId ); }
		}
 
		public RecurringTaskSchedule TaskSchedule {
			get{ return( mTaskSchedule ); }
		}

		internal void Execute() {
			mAction( this );
		}
	}
}
