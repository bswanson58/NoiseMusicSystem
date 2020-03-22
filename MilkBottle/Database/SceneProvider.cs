using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;

namespace MilkBottle.Database {
    class SceneProvider :EntityProvider<PresetScene>, ISceneProvider {
        public SceneProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.SceneCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetScene>().Id( e => e.Id );
        }

        public Either<Exception, Unit> SelectScenes( Action<IEnumerable<PresetScene>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Unit> Insert( PresetScene scene ) {
            return InsertEntity( scene );
        }

        public Either<Exception, Unit> Update( PresetScene scene ) {
            return UpdateEntity( scene );
        }

        public Either<Exception, Unit> Delete( PresetScene scene ) {
            return DeleteEntity( scene );
        }
    }
}
