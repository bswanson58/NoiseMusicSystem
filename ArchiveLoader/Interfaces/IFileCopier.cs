using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveLoader.Interfaces {
    public class FileCopyStatus {
        public string   FileName { get; }
        public bool     CopyCompleted { get; }
        public bool     Success { get; }
        public string   ErrorMessage { get; }

        public FileCopyStatus( string fileName ) {
            FileName = fileName;
            CopyCompleted = false;
            Success = true;
        }

        public FileCopyStatus( bool copyCompleted ) {
            FileName = String.Empty;
            CopyCompleted = copyCompleted;
            Success = true;
        }

        public FileCopyStatus( Exception ex ) {
            FileName = String.Empty;
            CopyCompleted = true;
            Success = false;
            ErrorMessage = ex.Message;
        }
    }

    interface IFileCopier {
        Task    CopyFiles( string sourceDirectory, string targetDirectory, IProgress<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation );
    }
}
