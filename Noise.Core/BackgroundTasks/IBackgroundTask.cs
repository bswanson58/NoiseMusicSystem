namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTask {
		string		TaskId { get; }

		void		ExecuteTask();
	}
}
