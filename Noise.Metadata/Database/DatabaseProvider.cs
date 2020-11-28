using System.IO;
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
            return mDatabase ?? ( mDatabase = new LiteDatabase( DatabasePath()));
        }

        private string DatabasePath() {
            var metaDataFolder = Path.Combine( mEnvironment.LibraryDirectory(), Constants.MetadataDirectory );

            if(!Directory.Exists( metaDataFolder )) {
                Directory.CreateDirectory( metaDataFolder );
            }

            return Path.Combine( metaDataFolder, Constants.MetadataDatabaseName );
        }

        public void Shutdown() {
            mDatabase?.Dispose();
            mDatabase = null;
        }
    }
}
