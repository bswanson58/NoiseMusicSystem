using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;

namespace MilkBottle.Database {
    class PresetProvider : EntityProvider<Preset>, IPresetProvider {
        public PresetProvider( IDatabaseProvider databaseProvider ) :
            base( databaseProvider, EntityCollection.PresetCollection ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Preset>().Id( e => e.Id );
            BsonMapper.Global.Entity<Preset>().DbRef( p => p.ParentLibrary, EntityCollection.LibraryCollection );
            BsonMapper.Global.Entity<Preset>().DbRef( t => t.Tags, EntityCollection.TagCollection );
/*
            var presets = db.GetCollection( EntityCollection.PresetCollection );
            var entities = presets.FindAll();

            entities.ForEach( entity => {
                if(!entity.ContainsKey( nameof( Preset.IsDuplicate ))) {
                    entity[nameof( Preset.IsDuplicate )] = false;
                }

                presets.Update( entity );
            }); */
        }

        protected override ILiteCollection<Preset> Include( ILiteCollection<Preset> list ) {
            return list
                .Include( p => p.ParentLibrary )
                .Include( p => p.Tags );
        }

        public Either<Exception, Option<Preset>> GetPresetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Unit> SelectPresets( Action<IEnumerable<Preset>> action ) {
            return SelectEntities( action );
        }

        public Either<Exception, Unit> SelectPresets( PresetLibrary forLibrary, Action<IEnumerable<Preset>> action ) {
            return SelectEntities( list => action( from p in list where p.ParentLibrary?.Id == forLibrary?.Id select p ));
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

        public Task<Either<Exception, Unit>> UpdateAll( Preset preset ) {
            return Task.Run( () => UpdateDuplicates( preset ));
        }

        private Either<Exception, Unit> UpdateDuplicates( Preset preset ) {
            var retValue = Update( preset );

            if(( retValue.IsRight ) &&
               ( preset.IsDuplicate )) {
                var duplicateList = new List<Preset>();

                retValue.Bind( unit => SelectPresets( list => duplicateList.AddRange( from p in list where p.Name.Equals( preset.Name ) select p )));

                duplicateList.ForEach( p => {
                    var updatedPreset = p.WithFavorite( preset.IsFavorite ).WithRating( preset.Rating ).WithTags( preset.Tags ).WithDuplicate( true );

                    retValue.Bind( unit => Update( updatedPreset ));
                });
            }

            return retValue;
        }

        public Either<Exception, Unit> Delete( Preset preset ) {
            return DeleteEntity( preset );
        }
    }
}
