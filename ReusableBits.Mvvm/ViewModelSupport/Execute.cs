using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

// From the Caliburn framework.
namespace ReusableBits.Mvvm.ViewModelSupport {
	/// <summary>
	///   Enables easy marshalling of code to the UI thread.
	/// </summary>
	public static class Execute {
		static bool?			mInDesignMode;
		static Action<Action>	mExecutor = action => action();

		/// <summary>
		///   Indicates whether or not the framework is in design-time mode.
		/// </summary>
		public static bool InDesignMode {
			get {
				if( mInDesignMode == null ) {
#if SILVERLIGHT
                    inDesignMode = DesignerProperties.IsInDesignTool;
#else
					var prop = DesignerProperties.IsInDesignModeProperty;
					mInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty( prop, typeof( FrameworkElement )).Metadata.DefaultValue;

					if( !mInDesignMode.GetValueOrDefault( false ) && Process.GetCurrentProcess().ProcessName.StartsWith( "devenv", StringComparison.Ordinal ) )
						mInDesignMode = true;
#endif
				}

				return mInDesignMode.GetValueOrDefault( false );
			}
		}

		/// <summary>
		///   Initializes the framework using the current dispatcher.
		/// </summary>
		public static void InitializeWithDispatcher() {
#if SILVERLIGHT
            var dispatcher = Deployment.Current.Dispatcher;

            executor = action => {
                if(dispatcher.CheckAccess())
                    action();
                else {
                    var waitHandle = new ManualResetEvent(false);
                    Exception exception = null;
                    dispatcher.BeginInvoke(() => {
                        try {
                            action();
                        }
                        catch(Exception ex) {
                            exception = ex;
                        }
                        waitHandle.Set();
                    });
                    waitHandle.WaitOne();
                    if(exception != null)
                        throw new TargetInvocationException("An error occurred while dispatching a call to the UI Thread", exception);
                }
            };
#else
			var dispatcher = Dispatcher.CurrentDispatcher;

			mExecutor = action => {
				if( dispatcher.CheckAccess())
					action();
				else dispatcher.Invoke( action );
			};
#endif
		}

		/// <summary>
		///   Resets the executor to use a non-dispatcher-based action executor.
		/// </summary>
		public static void ResetWithoutDispatcher() {
			mExecutor = action => action();
		}

		/// <summary>
		///   Executes the action on the UI thread.
		/// </summary>
		/// <param name = "action">The action to execute.</param>
		public static void OnUIThread( this Action action ) {
			mExecutor( action );
		}
	}
}
