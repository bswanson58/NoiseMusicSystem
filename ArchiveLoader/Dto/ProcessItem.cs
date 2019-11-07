using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Dto {
    public enum EventReason {
        Add,
        Completed,
        Update
    }

    [DebuggerDisplay("ProcessItem: {" + nameof( Name ) + "}" )]
    public class ProcessItem {
        public  string      Key {  get; }
        public  string      Name { get; }
        public  string      Artist { get; }
        public  string      Album { get; }
        public  string      TrackName { get; }
        public  string      FileName { get; }

        public  IList<ProcessHandler>   ProcessList { get; }

        public ProcessItem( FileCopyStatus fromStatus ) {
            Key = Guid.NewGuid().ToString( "N" );
            FileName = fromStatus.FileName;
            Artist = fromStatus.Artist;
            Album = fromStatus.Album;
            Name = Path.GetFileName( FileName );
            TrackName = Path.GetFileNameWithoutExtension( Name );

            ProcessList = new List<ProcessHandler>();
        }

        public ProcessHandler FindRunnableProcess() {
            var retValue = default( ProcessHandler );

            if( ProcessList.All( i => i.ProcessState != ProcessState.Running )) {
                retValue = ProcessList.FirstOrDefault( i => i.ProcessState == ProcessState.Pending && !String.IsNullOrWhiteSpace( i.Handler.ExePath ));
            }

            return retValue;
        }

        public bool HasCompletedProcessing() {
            return !ProcessList.Any() || ProcessList.All( h => h.ProcessState == ProcessState.Completed );
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
