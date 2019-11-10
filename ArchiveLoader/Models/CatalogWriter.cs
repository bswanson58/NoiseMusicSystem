using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class CatalogWriter : ICatalogWriter {
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;

        public CatalogWriter( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        public void CreateCatalog( string volumeName, IEnumerable<CompletedProcessItem> items ) {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();
            var reportFile = Path.ChangeExtension( Path.Combine(preferences.CatalogDirectory, volumeName ), ".xml" );

            if (volumeName.Contains(Path.VolumeSeparatorChar.ToString())) {
                reportFile = Path.ChangeExtension( Path.Combine( preferences.CatalogDirectory, Path.GetFileNameWithoutExtension( volumeName )), ".xml" );
            }

            Task.Run(() => { WriteCatalog( reportFile, items ); });
        }

        private void WriteCatalog( string toFile, IEnumerable<CompletedProcessItem> items ) {
            try {
                var directory = Path.GetDirectoryName( toFile );

                if ((!String.IsNullOrWhiteSpace( directory )) &&
                    (!Directory.Exists( directory ))) {
                    Directory.CreateDirectory( directory );
                }

                using ( var xmlWriter = XmlWriter.Create( toFile, new XmlWriterSettings { Indent = true, })) {
                    xmlWriter.WriteStartElement( "Catalog" );

                    OutputArtists( xmlWriter, items.ToList());

                    xmlWriter.WriteEndElement(); // Catalog
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"CatalogWriter writing catalog to: '{toFile}'", ex );
            }
        }

        private void OutputArtists( XmlWriter xmlWriter, IList<CompletedProcessItem> items ) {
            var artistList = items.GroupBy( i => i.Artist ).Select( g => g.First().Artist );

            foreach( var artist in artistList ) {
                xmlWriter.WriteStartElement( "Artist" );
                xmlWriter.WriteAttributeString( "name", artist );

                OutputAlbums( xmlWriter, ( from i in items where i.Artist.Equals( artist ) select  i ).ToList());

                xmlWriter.WriteEndElement(); // Artist
            }
        }

        private void OutputAlbums( XmlWriter xmlWriter, IList<CompletedProcessItem> items ) {
            var albumList = items.GroupBy( i => i.Album ).Select( g => g.First().Album );

            foreach( var album in albumList ) {
                xmlWriter.WriteStartElement( "Album" );
                xmlWriter.WriteAttributeString( "name", album );

                OutputVolumes( xmlWriter, ( from i in items where i.Album.Equals( album ) select i ).ToList());

                xmlWriter.WriteEndElement(); // Album
            }
        }

        private void OutputVolumes( XmlWriter xmlWriter, IList<CompletedProcessItem> items ) {
            var volumes = items.GroupBy( i => i.Subdirectory ).Select( g => g.First().Subdirectory );

            foreach( var volume in volumes ) {
                if( String.IsNullOrWhiteSpace( volume )) {
                    OutputFiles( xmlWriter, from i in items where i.Subdirectory.Equals( volume ) select i );
                }
                else {
                    xmlWriter.WriteStartElement( "Volume" );
                    xmlWriter.WriteAttributeString( "name", volume );

                    OutputFiles( xmlWriter, from i in items where i.Subdirectory.Equals( volume ) select i );
                    xmlWriter.WriteEndElement(); // Volume
                }
            }
        }

        private void OutputFiles( XmlWriter xmlWriter, IEnumerable<CompletedProcessItem> items ) {
            foreach( var item in items ) {
                xmlWriter.WriteElementString( "File", Path.GetFileName( item.FileName ));
            }
        }
    }
}
