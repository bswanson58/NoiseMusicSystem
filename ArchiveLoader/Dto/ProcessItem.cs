using System.Collections.Generic;
using System.IO;

namespace ArchiveLoader.Dto {
    public enum EventReason {
        Add,
        Completed,
        Update
    }

    public class ProcessItem {
        public  string      Name { get; }
        public  string      FileName { get; }

        public  IList<ProcessHandler>   ProcessList { get; }

        public ProcessItem( string path ) {
            FileName = path;
            Name = Path.GetFileName( path );

            ProcessList = new List<ProcessHandler>();
        }
    }

    public class ProcessItemEvent {
        public  EventReason     Reason { get; }
        public  ProcessItem     Item { get; }

        public ProcessItemEvent( ProcessItem item, EventReason reason ) {
            Reason = reason;
            Item = item;
        }
    }
}
