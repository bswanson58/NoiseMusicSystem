using System;

namespace Noise.Core.Support.AsyncTask {
	public interface IAsyncTaskResult {
        void	Execute();

        event	EventHandler Completed;
	}
}
