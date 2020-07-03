using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetLibraryProvider : EntityProvider<PresetLibrary>, IPresetLibraryProvider {
        public PresetLibraryProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.LibraryCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetLibrary>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<PresetLibrary>> GetLibraryById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<PresetLibrary>> GetLibraryByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( PresetLibrary.Name ), validName )));
        }

        public Either<Exception, Unit> QueryLibraries( Action<ILiteQueryable<PresetLibrary>> action ) {
            return QueryEntities( action );
        }

        public Either<Exception, Unit> SelectLibraries( Action<IEnumerable<PresetLibrary>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Option<PresetLibrary>> FindLibrary( BsonExpression predicate ) {
            return FindEntity( predicate );
        }

        public Either<Exception, Unit> FindLibraryList( BsonExpression predicate, Action<IEnumerable<PresetLibrary>> action ) {
            return FindEntityList( predicate, action );
        }

        public Either<Exception, Unit> Insert( PresetLibrary library ) {
            return InsertEntity( library );
        }

        public Either<Exception, Unit> Update( PresetLibrary library ) {
            return UpdateEntity( library );
        }

        public Either<Exception, Unit> Delete( PresetLibrary library ) {
            return DeleteEntity( library );
        }
    }
}
