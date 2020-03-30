﻿using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetProvider : EntityProvider<Preset>, IPresetProvider {
        public PresetProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.PresetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Preset>().Id( e => e.Id );
            BsonMapper.Global.Entity<Preset>().DbRef( p => p.Library, EntityCollection.LibraryCollection );
            BsonMapper.Global.Entity<Preset>().DbRef( t => t.Tags, EntityCollection.TagCollection );

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        protected override ILiteCollection<Preset> Include( ILiteCollection<Preset> list ) {
            return list
                .Include( p => p.Library )
                .Include( p => p.Tags );
        }

        public Either<Exception, Option<Preset>> GetPresetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<Preset>> GetPresetByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( Preset.Name ), validName )));
        }

        public Either<Exception, Unit> SelectPresets( Action<IEnumerable<Preset>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Unit> SelectPresets( PresetLibrary forLibrary, Action<IEnumerable<Preset>> action ) {
            return SelectEntities( list => action( from p in list where p.Library?.Id == forLibrary?.Id select p ));
        }

        public Either<Exception, Unit> SelectPresets( PresetTag forTag, Action<IEnumerable<Preset>> action ) {
            return SelectEntities( list => action( from p in list where p.Tags.Any( t => t.Identity.Equals( forTag.Identity )) select p ));
        }

        public Either<Exception, Unit> FindPresetList( string predicate, Action<IEnumerable<Preset>> action ) {
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
