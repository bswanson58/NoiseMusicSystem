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
    class PresetProvider : EntityProvider<Preset>, IPresetProvider {
        public PresetProvider( IEnvironment environment ) :
            base( Path.Combine( environment.DatabaseDirectory(), ApplicationConstants.DatabaseFileName ), EntityCollection.PresetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Preset>().Id( e => e.Id );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<DatabaseError, Preset> GetPresetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<DatabaseError, Preset> GetPresetByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( Preset.Name ), validName )));
        }

        public Either<DatabaseError, Unit> QueryPresets( Action<ILiteQueryable<Preset>> action ) {
            return QueryEntities( action );
        }

        public Either<DatabaseError, Unit> SelectPresets( Action<IEnumerable<Preset>> action ) {
            return SelectEntities( action );
        }

        public Either<DatabaseError, Preset> FindPreset( BsonExpression predicate ) {
            return FindEntity( predicate );
        }

        public Either<DatabaseError, Unit> FindPresetList( BsonExpression predicate, Action<IEnumerable<Preset>> action ) {
            return FindEntityList( predicate, action );
        }

        public Either<DatabaseError, Unit> Insert( Preset preset ) {
            return InsertEntity( preset );
        }

        public Either<DatabaseError, Unit> Update( Preset preset ) {
            return UpdateEntity( preset );
        }

        public Either<DatabaseError, Unit> Delete( Preset preset ) {
            return DeleteEntity( preset );
        }
    }
}
