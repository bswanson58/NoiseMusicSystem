using System;
using System.Diagnostics;

namespace Noise.UI.Support {
	public class BindingErrorListener : TraceListener {
		private static BindingErrorListener mBindingListener;
		private Action<string> mLogAction;

		private static BindingErrorListener BindingListener {
			get { return mBindingListener ?? ( mBindingListener = new BindingErrorListener()); }
		}

		public static void Listen( Action<string> logAction ) {
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning;

			if( !PresentationTraceSources.DataBindingSource.Listeners.Contains( BindingListener )) {
				PresentationTraceSources.DataBindingSource.Listeners.Add( new BindingErrorListener { mLogAction = logAction } );
			}
		}

		public override void Write( string message ) { }
		public override void WriteLine( string message ) {
			mLogAction( message );
		}
	}
}
