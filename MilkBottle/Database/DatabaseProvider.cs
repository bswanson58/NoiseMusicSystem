using System;
using System.IO;
using LanguageExt;
using LiteDB;
using MilkBottle.Interfaces;
using MilkBottle.Properties;

namespace MilkBottle.Database {
    class DatabaseProvider : IDatabaseProvider {
        private readonly IEnvironment   mEnvironment;
        private LiteDatabase            mDatabase;

        public DatabaseProvider( IEnvironment environment ) {
            mEnvironment = environment;
        }

        public Either<Exception, LiteDatabase> GetDatabase() {
            return Prelude.Try( () => mDatabase ?? ( mDatabase = new LiteDatabase( DatabasePath()))).ToEither();
        }

        private string DatabasePath() {
            return Path.Combine( mEnvironment.DatabaseDirectory(), ApplicationConstants.DatabaseFileName );
        }

        public void Dispose() {
            mDatabase?.Dispose();
            mDatabase = null;
        }
    }
}
