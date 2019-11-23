using System;
using System.Collections.Generic;
using System.IO;
using Album4Matter.Dto;
using Album4Matter.Interfaces;

namespace Album4Matter.Models {
    class SourceScanner : ISourceScanner {

        public IEnumerable<SourceItem> CollectFolder( string rootPath, Action<SourceItem> onItemInspect ) {
            var rootFolder = new SourceFolder( rootPath, null );

            if( Directory.Exists( rootPath )) {
                CollectFolder( rootFolder, onItemInspect );
            }

            return rootFolder.Children;
        }

        private void CollectFolder( SourceFolder rootFolder, Action<SourceItem> onItemInspect ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var folder = new SourceFolder( directory, rootFolder.Key, onItemInspect );

                rootFolder.Children.Add( folder );
                CollectFolder( folder, onItemInspect );
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                rootFolder.Children.Add( new SourceFile( file, rootFolder.Key, onItemInspect ));
            }
        }
    }
}
