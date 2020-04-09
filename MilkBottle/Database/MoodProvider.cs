using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class MoodProvider : EntityProvider<Mood>, IMoodProvider {
        public MoodProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.MoodCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Mood>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<Mood>> GetMoodById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Unit> SelectMoods( Action<IEnumerable<Mood>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Unit> Insert( Mood mood ) {
            return InsertEntity( mood );
        }

        public Either<Exception, Unit> Update( Mood mood ) {
            return UpdateEntity( mood );
        }

        public Either<Exception, Unit> Delete( Mood mood ) {
            return DeleteEntity( mood );
        }
    }
}
