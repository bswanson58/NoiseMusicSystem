using ReusableBits.Threading;

namespace ReusableBits.Interfaces {
	public interface IRecurringTaskScheduler {
		void			AddRecurringTask( RecurringTask task );
		RecurringTask	RetrieveTask( string taskName );
		void			RemoveAllTasks();
		void			RemoveTask( string taskName );
	}
}