using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.UI.Support {
	public class TaskHandler<TResult> {
		private readonly TaskScheduler			mUiTaskScheduler;
		private readonly TaskScheduler			mTaskScheduler;

		public TaskHandler() :
			this( TaskScheduler.Default, TaskScheduler.FromCurrentSynchronizationContext()) {
		}

		public TaskHandler( TaskScheduler taskScheduler, TaskScheduler uiTaskScheduler ) {
			mTaskScheduler = taskScheduler;
			mUiTaskScheduler = uiTaskScheduler;
		}

		public void StartTask( Func<TResult> taskCode, Action<TResult> onCompletion, Action<Task> onFault ) {
			Task<TResult>.Factory.StartNew( taskCode, CancellationToken.None, TaskCreationOptions.None, mTaskScheduler )
				.ContinueWith( result => onCompletion( result.Result ), mUiTaskScheduler )
				.ContinueWith( result => onFault, TaskContinuationOptions.OnlyOnFaulted );
		}
	}
}
