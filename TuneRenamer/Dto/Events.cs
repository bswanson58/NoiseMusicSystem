using System.Windows;

namespace TuneRenamer.Dto {
    public class Events {
        public class StatusEvent {
            public	string			Message { get; private set; }
            public	bool			ExtendDisplay { get; set; }

            public StatusEvent( string message ) {
                Message = message;
            }
        }

        public class WindowStateEvent {
            public  WindowState     State { get; }

            public WindowStateEvent( WindowState state ) {
                State = state;
            }
        }
    }
}
