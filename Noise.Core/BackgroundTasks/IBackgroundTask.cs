using Microsoft.Practices.Unity;

namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTask {
		string		TaskId { get; }

		bool		Initialize( IUnityContainer container );

		void		ExecuteTask();
		void		Shutdown();
	}
}
