using System;
using System.Collections.Generic;
using System.IO;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Properties;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetProvider : EntityProvider<Preset>, IPresetProvider {
        public PresetProvider( IEnvironment environment ) :
            base( Path.Combine( environment.DatabaseDirectory(), ApplicationConstants.DatabaseFileName ), EntityCollection.PresetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Preset>().Id( e => e.Id );
            BsonMapper.Global.Entity<Preset>().DbRef( p => p.Library, EntityCollection.LibraryCollection );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<Preset>> GetPresetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<Preset>> GetPresetByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( Preset.Name ), validName )));
        }

        public Either<Exception, Unit> QueryPresets( Action<ILiteQueryable<Preset>> action ) {
            return QueryEntities( action );
        }

        public Either<Exception, Unit> SelectPresets( Action<IEnumerable<Preset>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Option<Preset>> FindPreset( BsonExpression predicate ) {
            return FindEntity( predicate );
        }

        public Either<Exception, Unit> FindPresetList( BsonExpression predicate, Action<IEnumerable<Preset>> action ) {
            return FindEntityList( predicate, action );
        }

        public Either<Exception, Unit> Insert( Preset preset ) {
            return InsertEntity( preset );
        }

        public Either<Exception, Unit> Update( Preset preset ) {
            return UpdateEntity( preset );
        }

        public Either<Exception, Unit> Delete( Preset preset ) {
            return DeleteEntity( preset );
        }
    }
}
