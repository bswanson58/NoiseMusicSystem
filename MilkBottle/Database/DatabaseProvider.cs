using System;
using System.IO;
using LanguageExt;
using LiteDB;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Database {
    class DatabaseProvider : IDatabaseProvider {
        private readonly IEnvironment           mEnvironment;
        private readonly IApplicationConstants  mApplicationConstants;
        private LiteDatabase                    mDatabase;

        public DatabaseProvider( IEnvironment environment, IApplicationConstants constants ) {
            mEnvironment = environment;
            mApplicationConstants = constants;
        }

        public Either<Exception, LiteDatabase> GetDatabase() {
            return Prelude.Try( () => mDatabase ?? ( mDatabase = new LiteDatabase( DatabasePath()))).ToEither();
        }

        private string DatabasePath() {
            return Path.Combine( mEnvironment.DatabaseDirectory(), mApplicationConstants.DatabaseFileName );
        }

        public void Dispose() {
            mDatabase?.Dispose();
            mDatabase = null;
        }
    }
}
