using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class TagProvider : EntityProvider<PresetTag>, ITagProvider {
        public TagProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.TagCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetTag>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<PresetTag>> GetTagById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<PresetTag>> GetTagByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( PresetTag.Name ), validName )));
        }

        public Either<Exception, Unit> SelectTags( Action<IEnumerable<PresetTag>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Unit> Insert( PresetTag tag ) {
            return InsertEntity( tag );
        }

        public Either<Exception, Unit> Update( PresetTag tag ) {
            return UpdateEntity( tag );
        }

        public Either<Exception, Unit> Delete( PresetTag tag ) {
            return DeleteEntity( tag );
        }
    }
}
