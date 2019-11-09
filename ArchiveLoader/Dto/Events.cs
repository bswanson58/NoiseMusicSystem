﻿namespace ArchiveLoader.Dto {
    public class Events {
        public class JobTargets {
            public string SourceDirectory { get; }
            public string TargetDirectory { get; }

            public JobTargets(string source, string target) {
                SourceDirectory = source;
                TargetDirectory = target;
            }
        }

        public class ProcessItemEvent {
            public CopyProcessEventReason  Reason { get; }
            public ProcessItem  Item { get; }

            public ProcessItemEvent(ProcessItem item, CopyProcessEventReason reason) {
                Reason = reason;
                Item = item;
            }
        }

        public class StatusEvent {
            public	string			Message { get; }
            public	bool			ExtendDisplay { get; set; }

            public StatusEvent( string message ) {
                Message = message;
            }
        }

    }
}
