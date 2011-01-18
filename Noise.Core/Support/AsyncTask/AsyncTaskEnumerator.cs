using System;
using System.Collections.Generic;

namespace Noise.Core.Support.AsyncTask {
	public class AsyncTaskEnumerator {
		private readonly IEnumerator<IAsyncTaskResult>	mAsyncEnum;

		public AsyncTaskEnumerator( IEnumerable<IAsyncTaskResult> children ) {
			mAsyncEnum = children.GetEnumerator();
		}

		public static void Begin( IAsyncTaskResult workflow ){
	        Begin( new [] { workflow }); 
		}
    
		public static void Begin( IEnumerable<IAsyncTaskResult> workflow ) {
			new AsyncTaskEnumerator( workflow ).Enumerate();
		}

		public void Enumerate() {
			ChildCompleted( null, EventArgs.Empty );
		}

		private void ChildCompleted( object sender, EventArgs args ) {
			var previous = sender as IAsyncTaskResult;

			if( previous != null )
				previous.Completed -= ChildCompleted;

			if( !mAsyncEnum.MoveNext() )
				return;

			var next = mAsyncEnum.Current;
			next.Completed += ChildCompleted;
			next.Execute();
		}
	}
}
