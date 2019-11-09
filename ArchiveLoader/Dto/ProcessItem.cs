using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Dto {
    public enum CopyProcessEventReason {
        Add,
        Completed,
        Update,
        CopyCompleted
    }

    [DebuggerDisplay("ProcessItem: {" + nameof( Name ) + "}" )]
    public class ProcessItem {
        public  string      Key {  get; }
        public  string      Name { get; }
        public  string      VolumeName { get; }
        public  string      ArtistFolder { get; }
        public  string      AlbumFolder { get; }
        public  string      FileName { get; }

        public  IDictionary<string, string> Metadata { get; }
        public  IList<ProcessHandler>       ProcessList { get; }

        public ProcessItem( string volumeName, FileCopyStatus fromStatus ) {
            VolumeName = volumeName;
            Key = Guid.NewGuid().ToString( "N" );
            FileName = fromStatus.FileName;
            ArtistFolder = fromStatus.ArtistFolder;
            AlbumFolder = fromStatus.AlbumFolder;
            Name = Path.GetFileName( FileName );

            ProcessList = new List<ProcessHandler>();
            Metadata = new Dictionary<string, string>();
        }

        public ProcessHandler FindRunnableProcess() {
            var retValue = default( ProcessHandler );

            if( ProcessList.All( i => i.ProcessState != ProcessState.Running )) {
                retValue = ProcessList.FirstOrDefault( i => i.ProcessState == ProcessState.Pending && i.Handler.IsExecutable && !String.IsNullOrWhiteSpace( i.Handler.ExePath ));
            }

            return retValue;
        }

        public bool HasCompletedProcessing() {
            return !ProcessList.Any() || ProcessList.All( h => h.ProcessState == ProcessState.Completed );
        }
    }
}
