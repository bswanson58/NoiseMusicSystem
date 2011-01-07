using System.ComponentModel.Composition;
using Microsoft.Practices.Unity;

namespace Noise.Core.BackgroundTasks {
	[Export( typeof( IBackgroundTask ))]
	public class ContentBuilder : IBackgroundTask {
		private IUnityContainer		mContainer;

		public string TaskId {
			get { return( "Task_ContentBuilder" ); }
		}

		public bool Initialize( IUnityContainer container ) {
			mContainer = container;

			return( true );
		}

		public void ExecuteTask() {
		}

		public void Shutdown() {
		}
	}
}
