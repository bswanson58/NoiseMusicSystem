using System;
using System.Diagnostics;

namespace Noise.UI.Support {
	public class BindingErrorListener : TraceListener {
		private Action<string> mLogAction;

		public static void Listen( Action<string> logAction ) {
			PresentationTraceSources.DataBindingSource.Listeners.Add( new BindingErrorListener { mLogAction = logAction } );
		}

		public override void Write( string message ) { }
		public override void WriteLine( string message ) {
			mLogAction( message );
		}
	}
}
