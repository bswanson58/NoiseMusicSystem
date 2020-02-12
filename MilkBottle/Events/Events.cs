namespace MilkBottle.Events {
    public class ApplicationClosing { }

    public class StatusEvent {
        public	string			Message { get; }
        public	bool			ExtendDisplay { get; set; }

        public StatusEvent( string message ) {
            Message = message;
        }
    }

    public class MilkInitialized { }
}
