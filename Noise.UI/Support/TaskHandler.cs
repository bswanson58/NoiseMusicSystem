﻿using System;
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

		public void StartTask( Func<TResult> taskCode, Action<TResult> onCompletion, Action<Exception> onFault ) {
			Task<TResult>.Factory.StartNew( taskCode, CancellationToken.None, TaskCreationOptions.None, mTaskScheduler )
				.ContinueWith( result => {
					if( result.Exception != null ) {
						HandleFault( result, onFault );
					}
					else {
						if(!Equals( result.Result, default( TResult ))) {
							onCompletion( result.Result );
						}
					} 
				}, mUiTaskScheduler );
		}

		private void HandleFault( Task<TResult> task, Action<Exception> faultHandler ) {
			if(( task.Exception != null ) &&
			   ( faultHandler != null )) {
				foreach( Exception ex in task.Exception.Flatten().InnerExceptions ) {
					faultHandler( ex );
				}
			}
		}
	}
}
