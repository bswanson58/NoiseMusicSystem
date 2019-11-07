using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveLoader.Interfaces {
    public enum FileCopyState {
        Discovered,
        Copying,
        Completed
    }

    public class FileCopyStatus {
        public string           FileName { get; }
        public string           Artist { get; }
        public string           Album { get; }
        public FileCopyState    Status { get; }
        public bool             Success { get; }
        public string           ErrorMessage { get; }
        public bool             CopyCompleted {  get; }

        public FileCopyStatus( string fileName ) {
            FileName = fileName;
            Artist = String.Empty;
            Album = String.Empty;
            Status = FileCopyState.Discovered;
            Success = true;
            CopyCompleted = false;
        }

        public FileCopyStatus( string fileName, FileCopyState state, IList<string> directoryList ) :
            this( fileName ) {
            Status = state;

            if( directoryList.Count > 1 ) {
                Artist = directoryList[0];
                Album = directoryList[1];
            }
        }

        public FileCopyStatus( string fileName, Exception ex ) :
            this( fileName ) {
            Status = FileCopyState.Completed;
            Success = false;
            ErrorMessage = ex.Message;
        }

        public FileCopyStatus( bool completed ) :
            this( String.Empty ) {
            CopyCompleted = completed;
        }
    }

    interface IFileCopier {
        Task    CopyFiles( string sourceDirectory, string targetDirectory, IProgress<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation );
    }
}
