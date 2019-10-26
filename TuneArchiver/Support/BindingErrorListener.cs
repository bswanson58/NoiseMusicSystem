using System;
using System.Diagnostics;

namespace TuneArchiver.Support {
    public class BindingErrorListener : TraceListener {
        private static BindingErrorListener mBindingListener;
        private static BindingErrorListener BindingListener => mBindingListener ?? ( mBindingListener = new BindingErrorListener());

        private Action<string>              mLogAction;

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
