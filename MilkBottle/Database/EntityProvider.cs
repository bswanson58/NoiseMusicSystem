using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;

namespace MilkBottle.Database {
    class EntityProvider<T> : ValidationBase<T> where T : EntityBase {
        private readonly IDatabaseProvider  mDatabaseProvider;
        private readonly string             mCollectionName;
        private LiteDatabase                mDatabase;

        protected EntityProvider( IDatabaseProvider databaseProvider, string collectionName ) {
            mDatabaseProvider = databaseProvider;
            mCollectionName = collectionName;
        }

        protected Either<Exception, LiteDatabase> CreateConnection() {
            if( mDatabase == null ) {
                return mDatabaseProvider.GetDatabase()
                    .Map( db => {
                        mDatabase = db;

                        InitializeDatabase( mDatabase );
                        
                        return mDatabase;
                    });
            }

            return mDatabase;
        }

        protected virtual void InitializeDatabase( LiteDatabase db ) { }
        protected virtual ILiteCollection<T> Include( ILiteCollection<T> list ) {
            return list;
        }

        private Try<Unit> ScanCollection( LiteDatabase db, Action<ILiteCollection<T>> withAction ) {
            return Prelude.Try( () => {
                withAction( Include( db.GetCollection<T>( mCollectionName )));

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> WithCollection( Action<ILiteCollection<T>> action ) {
            return ValidateAction( action )
                .Bind( validAction => CreateConnection())
                    .Bind( db => ScanCollection( db, action ).ToEither());
        }

        private TryOption<T> FindWithExpression( BsonExpression expression ) {
            return Prelude.TryOption( () => {
                T retValue = default;

                WithCollection( collection => {
                    retValue = collection.FindOne( expression );
                });

                return retValue;
            });
        }

        protected Either<Exception, Option<T>> FindEntity( BsonExpression expression ) {
            return FindWithExpression( expression ).ToEither();
        }

        private Try<Unit> FindListWithExpression( BsonExpression expression, Action<IEnumerable<T>> action ) {
            return Prelude.Try( () => {
                WithCollection( c => action( c.Find( expression )));

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> FindEntityList( BsonExpression expression, Action<IEnumerable<T>> action ) {
            return ValidateAction( action )
                .Bind( a => FindListWithExpression( expression, action ).ToEither());
        }

        private Try<Unit> QueryEntities( LiteDatabase db, Action<ILiteQueryable<T>> queryAction ) {
            return Prelude.Try( () => {
                queryAction( Include( db.GetCollection<T>( mCollectionName )).Query());

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> QueryEntities( Action<ILiteQueryable<T>> action ) {
            return ValidateAction( action )
                .Bind( validAction => CreateConnection())
                    .Bind( db => QueryEntities( db, action ).ToEither());
        }

        private Try<Unit> SelectEntities( LiteDatabase db, Action<IEnumerable<T>> withAction ) {
            return Prelude.Try(  () => {
                withAction( Include( db.GetCollection<T>( mCollectionName )).FindAll());

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> SelectEntities( Action<IEnumerable<T>> action ) {
            return ValidateAction( action )
                .Bind( validAction => CreateConnection())
                    .Bind( db => SelectEntities( db, action ).ToEither());
        }

        private TryOption<T> GetEntityById( LiteDatabase db, ObjectId id ) {
            return Prelude.TryOption( () => Include( db.GetCollection<T>( mCollectionName )).FindById( id ));
        }

        protected Either<Exception, Option<T>> GetEntityById( ObjectId id ) {
            return ValidateObjectId( id )
                .Bind( validId => CreateConnection())
                    .Bind( db => GetEntityById( db, id ).ToEither());
        }

        private Try<Unit> InsertEntity( LiteDatabase db, T entity ) {
            return Prelude.Try( () => {
                db.GetCollection<T>( mCollectionName ).Insert( entity );

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> InsertEntity( T entity ) {
            return ValidateEntity( entity )
                .Bind( e => CreateConnection())
                    .Bind( db => InsertEntity( db, entity ).ToEither());
        }

        private Try<Unit> UpdateEntity( LiteDatabase db, T entity ) {
            return Prelude.Try( () => {
                if(!db.GetCollection<T>( mCollectionName ).Update( entity )) {
                    throw new DatabaseException( "Entity to update was not found" );
                }

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> UpdateEntity( T entity ) {
            return ValidateEntity( entity )
                .Bind( e => CreateConnection())
                    .Bind( db => UpdateEntity( db, entity ).ToEither());
        }

        private Try<Unit> DeleteEntity( LiteDatabase db, T entity ) {
            return Prelude.Try( () => {
                if(!db.GetCollection<T>( mCollectionName ).Delete( entity.Id )) {
                    throw new DatabaseException( "Entity to delete was not found" );
                }

                return Unit.Default;
            });
        }

        protected Either<Exception, Unit> DeleteEntity( T entity ) {
            return ValidateEntity( entity )
                .Bind( e => CreateConnection())
                    .Bind( db => DeleteEntity( db, entity ).ToEither());
        }
    }
}
