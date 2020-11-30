using System.IO;
using Ionic.Zip;
using LiteDB;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.Database {
    class DatabaseProvider : IDatabaseProvider {
        private readonly INoiseEnvironment  mEnvironment;
        private LiteDatabase                mDatabase;

        public DatabaseProvider( INoiseEnvironment environment ) {
            mEnvironment = environment;
        }

        public LiteDatabase GetDatabase() {
            return mDatabase ?? ( mDatabase = new LiteDatabase( DatabaseFile()));
        }

        private string DatabasePath() {
            var metaDataFolder = Path.Combine( mEnvironment.LibraryDirectory(), Constants.MetadataDirectory );

            if(!Directory.Exists( metaDataFolder )) {
                Directory.CreateDirectory( metaDataFolder );
            }

            return metaDataFolder;
        }

        private string DatabaseFile() {
            return Path.Combine( DatabasePath(), Constants.MetadataDatabaseName );
        }

        public void ExportMetadata( string exportPath ) {
            Shutdown();

            using( var zipFile = new ZipFile( exportPath )) {
                zipFile.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zipFile.AddDirectory( DatabasePath());

                zipFile.Save();
            }
        }

        public void ImportMetadata( string importPath ) {
            Shutdown();

            using( var zipFile = ZipFile.Read( importPath )) {
                zipFile.UseZip64WhenSaving = Zip64Option.AsNecessary;

                zipFile.ExtractAll( DatabasePath(), ExtractExistingFileAction.OverwriteSilently );
            }
        }

        public void Shutdown() {
            mDatabase?.Dispose();
            mDatabase = null;
        }
    }
}
