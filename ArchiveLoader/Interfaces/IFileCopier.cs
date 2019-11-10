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
        public string           ArtistFolder { get; }
        public string           AlbumFolder { get; }
        public string           Subdirectory { get; }
        public FileCopyState    Status { get; }
        public bool             Success { get; }
        public string           ErrorMessage { get; }
        public bool             CopyCompleted {  get; }

        public FileCopyStatus( string fileName ) {
            FileName = fileName;
            ArtistFolder = String.Empty;
            AlbumFolder = String.Empty;
            Subdirectory = String.Empty;
            Status = FileCopyState.Discovered;
            Success = true;
            CopyCompleted = false;
        }

        public FileCopyStatus( string fileName, FileCopyState state, IList<string> directoryList ) :
            this( fileName ) {
            Status = state;

            if( directoryList.Count > 1 ) {
                ArtistFolder = directoryList[0];
                AlbumFolder = directoryList[1];

                if( directoryList.Count > 2 ) {
                    Subdirectory = directoryList[2];
                }
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
        Task    CopyFiles( string sourceDirectory, string targetDirectory, Action<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation );
    }
}
