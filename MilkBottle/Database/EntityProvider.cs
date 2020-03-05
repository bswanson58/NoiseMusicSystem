using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Types;

namespace MilkBottle.Database {
    class EntityProvider<T> : IDisposable where T : EntityBase {
        private readonly string         mDatabasePath;
        private readonly string         mCollectionName;
        private LiteDatabase            mDatabase;

        protected EntityProvider( string databasePath, string collectionName ) {
            mDatabasePath = databasePath;
            mCollectionName = collectionName;
        }

        protected Option<T2> ElevateEntity<T2>( T2 entity ) {
            return entity != null ? Option<T2>.Some( entity ) : Option<T2>.None;
        }

        protected Either<DatabaseError, LiteDatabase> CreateConnection() {
            try {
                if( mDatabase == null ) {
                    mDatabase = new LiteDatabase( mDatabasePath );

                    InitializeDatabase( mDatabase );
                }

                return mDatabase;
            }
            catch( Exception ex ) {
                return new DatabaseError( "Opening database", ex );
            }
        }

        protected virtual void InitializeDatabase( LiteDatabase db ) { }

        protected Either<DatabaseError, Unit> WithCollection( Action<ILiteCollection<T>> action ) {
            return ValidateAction( action ).Bind( validAction => {
                try {
                    return CreateConnection().Map( db => {
                        action( db.GetCollection<T>( mCollectionName ));

                        return Unit.Default;
                    });
                }
                catch( Exception ex ) {
                    return new DatabaseError( "Database Initialize", ex );
                }
            });
        }

        protected Either<DatabaseError, T> FindEntity( BsonExpression predicate ) {
            return ElevateEntity( predicate ).Match(
                    FindWithPredicate,
                    () => new DatabaseError( "Predicate cannot be null" ));
        }

        private Either<DatabaseError, T> FindWithPredicate( BsonExpression predicate ) {
            T retValue = default;

            WithCollection( collection => {
                retValue = collection.FindOne( predicate );
            });

            if( retValue != null ) {
                return retValue;
            }

            return new DatabaseError( "Entity was not located" );
        }

        protected Either<DatabaseError, Unit> FindEntityList( BsonExpression predicate, Action<IEnumerable<T>> action ) {
            return ElevateEntity( predicate ).Match(
                p => FindListWithPredicate( p, action ),
                () => new DatabaseError( "Predicate cannot be null" ));
        }

        private Either<DatabaseError, Unit> FindListWithPredicate( BsonExpression predicate, Action<IEnumerable<T>> action ) {
            return ValidateAction( action  )
                .Bind( a => WithCollection( collection => a( collection.Find( predicate ))));
        }

        protected Either<DatabaseError, Unit> QueryEntities( Action<ILiteQueryable<T>> action ) {
            return ValidateAction( action ).Bind( validAction => {
                try {
                    return CreateConnection().Map( db => {
                        action( db.GetCollection<T>( mCollectionName ).Query());

                        return Unit.Default;
                    });
                }
                catch( Exception ex ) {
                    return new DatabaseError( "Database GetAllEntities", ex );
                }
            });
        }

        protected Either<DatabaseError, Unit> SelectEntities( Action<IEnumerable<T>> action ) {
            return ValidateAction( action ).Bind( validAction => {
                try {
                    return CreateConnection().Map( db => {
                        action( db.GetCollection<T>( mCollectionName ).FindAll());

                        return Unit.Default;
                    });
                }
                catch( Exception ex ) {
                    return new DatabaseError( "Database GetAllEntities", ex );
                }
            });
        }

        protected Either<DatabaseError, T> GetEntityById( ObjectId id ) {
            return ValidateObjectId( id ).Bind( validId => {
                try {
                    return CreateConnection().Map( db => db.GetCollection<T>( mCollectionName ).FindById( validId ));
                }
                catch( Exception ex ) {
                    return new DatabaseError( "Database GetEntityById", ex );
                }
            });
        }

        protected Either<DatabaseError, Unit> InsertEntity( T entity ) {
            return ElevateEntity( entity ).Match(
                e => {
                    try {
                        return CreateConnection().Map( db => {
                            db.GetCollection<T>( mCollectionName ).Insert( e );

                            return Unit.Default;
                        });
                    }
                    catch( Exception ex ) {
                        return new DatabaseError( "Database InsertEntity", ex );
                    }
                },

                () => new DatabaseError( "Entity cannot be null" ));
        }

        protected Either<DatabaseError, Unit> UpdateEntity( T entity ) {
            return ElevateEntity( entity ).Match(
                e => CreateConnection().Bind( db => UpdateEntity( db, e )),
                () => new DatabaseError( "Entity cannot be null" ));
        }

        private Either<DatabaseError, Unit> UpdateEntity( LiteDatabase db, T entity ) {
            try {
                if(!db.GetCollection<T>( mCollectionName ).Update( entity )) {
                    return new DatabaseError( "Entity was not located for update" );
                }

                return Unit.Default;
            }
            catch( Exception ex ) {
                return new DatabaseError( "Database UpdateEntity", ex );
            }
        }

        protected Either<DatabaseError, Unit> DeleteEntity( T entity ) {
            return ElevateEntity( entity ).Match( 
                e => CreateConnection().Bind( db => DeleteEntity( db, e )),
                () => new DatabaseError( "Entity cannot be null" ));
        }

        private Either<DatabaseError, Unit> DeleteEntity( LiteDatabase db, T entity ) {
            try {
                if(!db.GetCollection<T>( mCollectionName ).Delete( entity.Id )) {
                    return new DatabaseError( "Entity was not located for deletion" );
                }

                return Unit.Default;
            }
            catch( Exception ex ) {
                return new DatabaseError( "Database DeleteEntity", ex );
            }
        }

        private Either<DatabaseError, ObjectId> ValidateObjectId( ObjectId id ) {
            if( id == null ) {
                return new DatabaseError( "Object id is null" );
            }

            if( id.Equals( new ObjectId())) {
                return new DatabaseError( "ObjectId is not initialized" );
            }

            return id;
        }

        private Either<DatabaseError, Action<IEnumerable<T>>> ValidateAction( Action<IEnumerable<T>> action ) {
            if( action == null ) {
                return new DatabaseError( "Action cannot be null" );
            }

            return action;
        }

        private Either<DatabaseError, Action<ILiteQueryable<T>>> ValidateAction( Action<ILiteQueryable<T>> action ) {
            if( action == null ) {
                return new DatabaseError( "Action cannot be null" );
            }

            return action;
        }

        private Either<DatabaseError, Action<ILiteCollection<T>>> ValidateAction( Action<ILiteCollection<T>> action ) {
            if( action == null ) {
                return new DatabaseError( "Action cannot be null" );
            }

            return action;
        }

        protected Either<DatabaseError, string> ValidateString( string value ) {
            if( String.IsNullOrWhiteSpace( value )) {
                return new DatabaseError( "String value cannot be empty or null" );
            }

            return value;
        }

        public void Dispose() {
            mDatabase?.Dispose();
            mDatabase = null;
        }
    }
}
