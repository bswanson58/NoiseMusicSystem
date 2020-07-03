using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiveLoader.Interfaces;
using ArchiveLoader.Support;

namespace ArchiveLoader.Platform {
    class FileCopier : IFileCopier {
        private readonly IPlatformLog   mLog;

        public FileCopier( IPlatformLog log ) {
            mLog = log;
        }

        public Task<long> GetDirectorySize( string directory ) {
            return Task.Run( () => {
                var retValue = 0L;

                if( Directory.Exists( directory )) {
                    var dirInfo = new DirectoryInfo( directory );

                    retValue = dirInfo.GetDirectorySize( true );
                }
                return retValue;
            });
        }

        public Task CopyFiles( string sourceDirectory, string targetDirectory, Action<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation ) {
            return Task.Run( () => {
                if(( Directory.Exists( sourceDirectory )) &&
                   ( Directory.Exists( targetDirectory ))) {
                    var directoryList = new List<string>();

                    CopyFilesRecursively( new DirectoryInfo( sourceDirectory ), new DirectoryInfo( targetDirectory ), onFileCopied, cancellation, directoryList );

                    onFileCopied?.Invoke( new FileCopyStatus( true ));
                }
            });
        }

        private bool CopyFilesRecursively( DirectoryInfo source, DirectoryInfo target, Action<FileCopyStatus> onFileCopied, CancellationTokenSource cancellation, IList<string> directoryList ) {
            var retValue = true;

            foreach( var directory in source.GetDirectories()) {
                directoryList.Add( directory.Name );

                if(!CopyFilesRecursively( directory, target.CreateSubdirectory( directory.Name ), onFileCopied, cancellation, directoryList )) {
                    retValue = false;

                    break;
                }
            }

            foreach( var file in source.GetFiles()) {
                try {
                    var destinationPath = Path.Combine( target.FullName, file.Name );

                    onFileCopied?.Invoke( new FileCopyStatus( destinationPath, FileCopyState.Discovered, directoryList, file.Length ));

                    if( File.Exists( destinationPath )) {
                        File.SetAttributes( destinationPath, FileAttributes.Normal );
                        File.Delete( destinationPath );
                    }

                    onFileCopied?.Invoke( new FileCopyStatus( destinationPath, FileCopyState.Copying, directoryList, file.Length ));

                    file.CopyTo( destinationPath );

                    onFileCopied?.Invoke( new FileCopyStatus( destinationPath, FileCopyState.Completed, directoryList, file.Length ));
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Copying file '{file.Name}' to '{target.FullName}'.", ex );

                    onFileCopied?.Invoke( new FileCopyStatus( file.Name, ex ));

                    retValue = false;
                    break;
                }

                if( cancellation.IsCancellationRequested ) {
                    retValue = false;
                    onFileCopied?.Invoke( new FileCopyStatus( true ));

                    break;
                }
            }

            if( directoryList.Any()) {
                directoryList.Remove( directoryList.Last());
            }

            return retValue;
        }
    }
}
