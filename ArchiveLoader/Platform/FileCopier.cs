using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Platform {
    class FileCopier : IFileCopier {
        private readonly IPlatformLog   mLog;

        public FileCopier( IPlatformLog log ) {
            mLog = log;
        }

        public Task CopyFiles( string sourceDirectory, string targetDirectory, IProgress<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation ) {
            return Task.Run( () => {
                if(( Directory.Exists( sourceDirectory )) &&
                   ( Directory.Exists( targetDirectory ))) {
                    CopyFilesRecursively( new DirectoryInfo( sourceDirectory ), new DirectoryInfo( targetDirectory ), onFileCopied, cancellation );

                    onFileCopied?.Report( new FileCopyStatus( true ));
                }
            });
        }

        private bool CopyFilesRecursively( DirectoryInfo source, DirectoryInfo target, IProgress<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation ) {
            var retValue = true;

            foreach( var directory in source.GetDirectories()) {
                if(!CopyFilesRecursively( directory, target.CreateSubdirectory( directory.Name ), onFileCopied, cancellation )) {
                    retValue = false;

                    break;
                }
            }

            foreach( var file in source.GetFiles()) {
                try {
                    var destinationPath = Path.Combine(target.FullName, file.Name);

                    if( File.Exists( destinationPath )) {
                        File.SetAttributes( destinationPath, FileAttributes.Normal );
                        File.Delete( destinationPath );
                    }
                    file.CopyTo( destinationPath );

                    onFileCopied?.Report( new FileCopyStatus( destinationPath ));
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Copying file '{file.Name}' to '{target.FullName}'.", ex );

                    onFileCopied?.Report( new FileCopyStatus( ex ));

                    retValue = false;
                    break;
                }

                if( cancellation.IsCancellationRequested ) {
                    retValue = false;
                    onFileCopied?.Report( new FileCopyStatus( true ));

                    break;
                }
            }

            return retValue;
        }
    }
}
