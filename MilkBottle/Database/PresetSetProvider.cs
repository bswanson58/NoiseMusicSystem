using System;
using System.Collections.Generic;
using LanguageExt;
using LiteDB;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MoreLinq.Extensions;
using Query = LiteDB.Query;

namespace MilkBottle.Database {
    class PresetSetProvider : EntityProvider<PresetSet>, IPresetSetProvider {
        private readonly IPresetProvider    mPresetProvider;

        public PresetSetProvider( IDatabaseProvider databaseProvider, IPresetProvider presetProvider ) :
            base( databaseProvider, EntityCollection.SetCollection ) {
            mPresetProvider = presetProvider;
        }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<PresetSet>().Id( e => e.Id );
            BsonMapper.Global.EnumAsInteger = true;
            BsonMapper.Global.TrimWhitespace = false;

            WithCollection( collection => {
                collection.EnsureIndex( p => p.Name );
            });
        }

        public Either<Exception, Option<PresetSet>> GetSetById( ObjectId id ) {
            return GetEntityById( id );
        }

        public Either<Exception, Option<PresetSet>> GetSetByName( string name ) {
            return ValidateString( name ).Bind( validName => FindEntity( Query.EQ( nameof( PresetSet.Name ), validName )));
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

        public Either<Exception, Unit> GetPresetList( PresetSet forSet, Action<IEnumerable<Preset>> action ) {
            var expression = BuildExpression( forSet.Qualifiers );

            return mPresetProvider.FindPresetList( expression, action );
        }

        private string BuildExpression( List<SetQualifier> qualifiers ) {
            var qualifierParts = new List<string>();

            foreach( var qualifier in qualifiers ) {
                switch( qualifier.Operation ) {
                    case QualifierOperation.Equal:
                        qualifierParts.Add( Query.EQ( qualifier.Field.ToString(), qualifier.TypedValue()));
                        break;

                    case QualifierOperation.NotEqual:
                        qualifierParts.Add( Query.Not( qualifier.Field.ToString(), qualifier.TypedValue()));
                        break;

                    case QualifierOperation.Contains:
                        qualifierParts.Add( Query.Contains( qualifier.Field.ToString(), qualifier.TypedValue()));
                        break;

                    case QualifierOperation.HasMemberName:
                        qualifierParts.Add( BuildMemberQualifiers( $"$.{qualifier.Field.ToString()}[*].Name", qualifier.Value.Split( SetQualifier.cValueSeparator )));
                        break;
                }
            }

            // always eliminate presets marked as Do Not Play
            qualifierParts.Add( Query.Not( nameof( Preset.Rating ), PresetRating.DoNotPlayValue ));

            return String.Join( " AND ", qualifierParts );
        }

        private string BuildMemberQualifiers( string forMember, IEnumerable<string> qualifiers ) {
            var predicateList = new List<string>();

            qualifiers.ForEach( q => predicateList.Add( $"{forMember} ANY='{q}'" ));

            return "(" + String.Join( " OR ", predicateList ) + ")";
        }
    }
}
