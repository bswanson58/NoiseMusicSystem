using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;

namespace Noise.Infrastructure.Support {
	public class AsyncCommandCompleteEventArgs : EventArgs {
		public Exception Exception { get; private set; }

		public AsyncCommandCompleteEventArgs( Exception ex ) {
			Exception = ex;
		}
	}

	public class AsyncCommand<T> : ICommand where T : class {
		private readonly	Action<T>	mExecuteAction;
		private bool					mIsExecuting;

		public event EventHandler CanExecuteChanged;
		public event EventHandler ExecutionStarting;
		public event EventHandler<AsyncCommandCompleteEventArgs> ExecutionComplete;

		public AsyncCommand( Action<T> executeAction ) {
			mExecuteAction = executeAction;
		}

		public bool IsExecuting {
			get { return mIsExecuting; }
			private set {
				mIsExecuting = value;
				if( CanExecuteChanged != null )
					CanExecuteChanged( this, EventArgs.Empty );
			}
		}

		public void OnExecute( T parameter ) {
			if( mExecuteAction != null ) {
				mExecuteAction( parameter );
			}
		}

		public void Execute( object parameter ) {
			if( parameter is T ) {
				try {
					var typedParameter = parameter as T;

					IsExecuting = true;
					if( ExecutionStarting != null ) {
						ExecutionStarting( this, EventArgs.Empty );
					}

					var dispatcher = Dispatcher.CurrentDispatcher;
					ThreadPool.QueueUserWorkItem(
							obj => {
								try {
									OnExecute( typedParameter );
									if( ExecutionComplete != null )
										dispatcher.Invoke( DispatcherPriority.Normal,
												ExecutionComplete, this,
												new AsyncCommandCompleteEventArgs( null ));
								}
								catch( Exception ex ) {
									if( ExecutionComplete != null )
										dispatcher.Invoke( DispatcherPriority.Normal,
												ExecutionComplete, this,
												new AsyncCommandCompleteEventArgs( ex ));
								}
								finally {
									dispatcher.Invoke( DispatcherPriority.Normal, new Action( () => IsExecuting = false ));
								}
							} );
				}
				catch( Exception ex ) {
					IsExecuting = false;

					if( ExecutionComplete != null ) {
						ExecutionComplete( this, new AsyncCommandCompleteEventArgs( ex ) );
					}
				}
			}
		}

		public virtual bool CanExecute( object parameter ) {
			return !IsExecuting;
		}
	}
}
