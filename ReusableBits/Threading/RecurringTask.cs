using System;
using CuttingEdge.Conditions;

namespace ReusableBits.Threading {
	public class RecurringTask {
		private readonly string					mTaskId;
		private readonly Action					mAction;
		private readonly RecurringTaskSchedule	mTaskSchedule;

		public RecurringTask( Action action ) :
			this( action, Guid.NewGuid().ToString()) { }

		public RecurringTask( Action action, string taskId ) {
			Condition.Requires( action ).IsNotNull();

			mAction = action;
			mTaskId = taskId;
			mTaskSchedule = new RecurringTaskSchedule();
		}
 
		public RecurringTaskSchedule TaskSchedule {
			get{ return( mTaskSchedule ); }
		}

		public void Execute() {
			mAction();
		}
	}
}
