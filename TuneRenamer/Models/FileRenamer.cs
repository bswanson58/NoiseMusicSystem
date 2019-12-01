using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TuneRenamer.Dto;
using TuneRenamer.Interfaces;

namespace TuneRenamer.Models {
    class FileRenamer : IFileRenamer {
        private readonly IPlatformLog   mLog;

        public FileRenamer( IPlatformLog log ) {
            mLog = log;
        }

        public Task<bool> RenameFiles( IEnumerable<SourceFile> fileList ) {
            return Task.Run( () => {
                var retValue = true;

                foreach( var file in fileList ) {
                    if( file.WillBeRenamed ) {
                        try {
                            var renamePath = Path.GetDirectoryName( file.FileName );

                            if(!String.IsNullOrWhiteSpace( renamePath )) {
                                renamePath = Path.Combine( renamePath, file.ProposedName );

                                File.Move( file.FileName, renamePath );
                            }
                        }
                        catch( Exception ex ) {
                            mLog.LogException( $"RenameFile: '{file.FileName}'", ex );

                            retValue = false;
                        }
                    }
                }

                return retValue;
            });
        }
    }
}
