using LiteDB;
using Noise.Metadata.Dto;
using Noise.Metadata.Interfaces;

namespace Noise.Metadata.Database {
    class EntityProvider<T> where T : EntityBase {
        private readonly IDatabaseProvider  mDatabaseProvider;
        private readonly string             mCollectionName;
        private LiteDatabase                mDatabase;

        protected virtual void InitializeDatabase( LiteDatabase db ) { }

        protected EntityProvider( IDatabaseProvider databaseProvider, string collectionName ) {
            mDatabaseProvider = databaseProvider;
            mCollectionName = collectionName;
        }

        protected LiteDatabase CreateConnection() {
            if( mDatabase == null ) {
                mDatabase = mDatabaseProvider.GetDatabase();

                if( mDatabase != null ) {
                    InitializeDatabase( mDatabase );
                }
            }

            return mDatabase;
        }

        protected ILiteCollection<T> CreateCollection() {
            return CreateConnection()?.GetCollection<T>( mCollectionName );
        }

        protected void InsertEntity( T entity ) {
            CreateCollection()?.Insert( entity );
        }

        protected bool UpdateEntity( T entity ) {
            return CreateCollection()?.Update( entity ) == true;
        }

        protected bool DeleteEntity( T entity ) {
            return CreateCollection()?.Delete( entity.Id ) == true;
        }
    }
}
