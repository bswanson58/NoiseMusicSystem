namespace ArchiveLoader.Dto {
    public class Events {
        public class StatusEvent {
            public	string			Message { get; }
            public	bool			ExtendDisplay { get; set; }

            public StatusEvent( string message ) {
                Message = message;
            }
        }

    }
}
