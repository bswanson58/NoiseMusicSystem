using System;
using System.Collections.Generic;
using System.IO;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Properties;
using MilkBottle.Types;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetLibraryProvider : EntityProvider<PresetLibrary>, IPresetLibraryProvider {
        public PresetLibraryProvider( IEnvironment environment ) :
            base( Path.Combine( environment.DatabaseDirectory(), ApplicationConstants.DatabaseFileName ), EntityCollection.PresetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetLibrary>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<DatabaseError, PresetLibrary> GetLibraryById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<DatabaseError, PresetLibrary> GetLibraryByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( PresetLibrary.Name ), validName )));
        }

        public Either<DatabaseError, Unit> QueryLibraries( Action<ILiteQueryable<PresetLibrary>> action ) {
            return QueryEntities( action );
        }

        public Either<DatabaseError, Unit> SelectLibraries( Action<IEnumerable<PresetLibrary>> action ) {
            return SelectEntities( action );
        }

        public Either<DatabaseError, PresetLibrary> FindLibrary( BsonExpression predicate ) {
            return FindEntity( predicate );
        }

        public Either<DatabaseError, Unit> FindLibraryList( BsonExpression predicate, Action<IEnumerable<PresetLibrary>> action ) {
            return FindEntityList( predicate, action );
        }

        public Either<DatabaseError, Unit> Insert( PresetLibrary library ) {
            return InsertEntity( library );
        }

        public Either<DatabaseError, Unit> Update( PresetLibrary library ) {
            return UpdateEntity( library );
        }

        public Either<DatabaseError, Unit> Delete( PresetLibrary library ) {
            return DeleteEntity( library );
        }
    }
}
