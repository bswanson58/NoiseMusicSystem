using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessBuilder : IProcessBuilder {
        private readonly    IPreferences            mPreferences;
        private readonly    List<FileTypeHandler>   mFileTypeHandlers;

        public ProcessBuilder( IPreferences preferences ) {
            mPreferences = preferences;

            mFileTypeHandlers = new List<FileTypeHandler>();
        }

        private void LoadFileHandlers() {
            var handlers = mPreferences.Load<FileHandlerStorageList>();

            mFileTypeHandlers.Clear();
            mFileTypeHandlers.AddRange( handlers.Handlers );
        }

        public void BuildProcessList( ProcessItem item ) {
            var fileExtension = GetExtension( Path.GetExtension( item.FileName ));

            LoadFileHandlers();

            do {
                var handler = mFileTypeHandlers.FirstOrDefault( h => h.InputExtension.Equals( GetExtension( fileExtension )));

                if( handler != null ) {
                    var outputFile = Path.ChangeExtension( item.FileName, handler.OutputExtension );

                    item.ProcessList.Add( new ProcessHandler( handler, item.FileName, outputFile ));

                    fileExtension = GetExtension( handler.OutputExtension );
                }
                else {
                    break;
                }
            }
            while( true );
        }

        private string GetExtension( string path ) {
            return path.TrimStart( '.' );
        }
    }
}
