using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetSetProvider : EntityProvider<PresetSet>, IPresetSetProvider {
        public PresetSetProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.SetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetSet>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<PresetSet>> GetSetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<PresetSet>> GetSetByName( string name ) {
            return ValidateString( name ).Bind<Option<PresetSet>>( validName => FindEntity( Query.EQ( nameof( PresetSet.Name ), validName )));
        }

        public Either<Exception, Unit> QuerySets( Action<ILiteQueryable<PresetSet>> action ) {
            return QueryEntities( action );
        }

        public Either<Exception, Unit> SelectSets( Action<IEnumerable<PresetSet>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Option<PresetSet>> FindSet( BsonExpression predicate ) {
            return FindEntity( predicate );
        }

        public Either<Exception, Unit> FindSetList( BsonExpression predicate, Action<IEnumerable<PresetSet>> action ) {
            return FindEntityList( predicate, action );
        }

        public Either<Exception, Unit> Insert( PresetSet set ) {
            return InsertEntity( set );
        }

        public Either<Exception, Unit> Update( PresetSet set ) {
            return UpdateEntity( set );
        }

        public Either<Exception, Unit> Delete( PresetSet set ) {
            return DeleteEntity( set );
        }
    }
}
