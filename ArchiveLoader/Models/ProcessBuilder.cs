using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessBuilder : IProcessBuilder {
        private readonly    IPreferences            mPreferences;
        private readonly    IExitHandlerFactory     mExitHandlerFactory;
        private readonly    List<FileTypeHandler>   mFileTypeHandlers;

        public ProcessBuilder( IExitHandlerFactory exitHandlerFactory, IPreferences preferences ) {
            mExitHandlerFactory = exitHandlerFactory;
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
            var inputFileName = item.FileName;

            LoadFileHandlers();

            var copyHandler = new CopyFileHandler();

            item.ProcessList.Add( new ProcessHandler( item.Key, copyHandler, inputFileName, inputFileName, mExitHandlerFactory.GetExitHandler( copyHandler )));

            do {
                var handler = mFileTypeHandlers.FirstOrDefault( h => h.InputExtension.Equals( GetExtension( fileExtension )));

                if( handler != null ) {
                    var outputFileName = Path.ChangeExtension( inputFileName, handler.OutputExtension );

                    item.ProcessList.Add( new ProcessHandler( item.Key, handler, inputFileName, outputFileName, mExitHandlerFactory.GetExitHandler( handler )));
                    inputFileName = outputFileName;

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
