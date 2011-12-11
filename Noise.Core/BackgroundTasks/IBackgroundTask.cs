using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	public interface IBackgroundTask {
		string		TaskId { get; }

		bool		Initialize( IUnityContainer container, IDatabaseManager databaseManager );

		void		ExecuteTask();
		void		Shutdown();
	}
}
