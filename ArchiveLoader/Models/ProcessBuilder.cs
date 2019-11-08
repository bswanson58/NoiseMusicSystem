using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ProcessBuilder : IProcessBuilder {
        private readonly    IPreferences            mPreferences;
        private readonly    IExitHandlerFactory     mExitHandlerFactory;
        private readonly    IFileMetadata           mFileMetadata;
        private readonly    List<FileTypeHandler>   mFileTypeHandlers;

        public ProcessBuilder( IFileMetadata metadata, IExitHandlerFactory exitHandlerFactory, IPreferences preferences ) {
            mFileMetadata = metadata;
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

            item.Metadata.Add(FileMetadata.cArtistMetadataTag, mFileMetadata.GetAlbumNameFromAlbum(item.ArtistFolder));
            item.Metadata.Add(FileMetadata.cAlbumMetadataTag, mFileMetadata.GetAlbumNameFromAlbum(item.AlbumFolder));
            item.Metadata.Add(FileMetadata.cPublishedMetadataTag, mFileMetadata.GetPublishedYearFromAlbum(item.AlbumFolder).ToString());
            item.Metadata.Add(FileMetadata.cTrackMetadataTag, mFileMetadata.GetTrackNameFromFileName(item.FileName));
            item.Metadata.Add(FileMetadata.cTrackNumberMetadataTag, mFileMetadata.GetTrackNumberFromFileName(item.FileName).ToString());

            LoadFileHandlers();

            // Create the dummy copy process and assume it starts running.
            var copyHandler = new CopyFileHandler();
            var copyHandlerProcess = new ProcessHandler( item, copyHandler, inputFileName, inputFileName, mExitHandlerFactory.GetExitHandler( copyHandler ));

            copyHandlerProcess.SetProcessRunning();
            item.ProcessList.Add( copyHandlerProcess );

            do {
                var handler = mFileTypeHandlers.FirstOrDefault( h => h.InputExtension.Equals( GetExtension( fileExtension )));

                if( handler != null ) {
                    var outputFileName = Path.ChangeExtension( inputFileName, handler.OutputExtension );

                    item.ProcessList.Add( new ProcessHandler( item, handler, inputFileName, outputFileName, mExitHandlerFactory.GetExitHandler( handler )));
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
