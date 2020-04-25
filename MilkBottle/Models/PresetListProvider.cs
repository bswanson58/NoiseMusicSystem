using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MoreLinq;

namespace MilkBottle.Models {
    class PresetListProvider : IPresetListProvider {
        private readonly IPresetLibraryProvider mLibraryProvider;
        private readonly IPresetSetProvider     mSetProvider;
        private readonly IPresetProvider        mPresetProvider;
        private readonly ITagProvider           mTagProvider;
        private readonly IPlatformLog           mLog;

        public PresetListProvider( IPresetLibraryProvider libraryProvider, IPresetSetProvider setProvider, IPresetProvider presetProvider, ITagProvider tagProvider,
                                   IPlatformLog log ) {
            mLibraryProvider = libraryProvider;
            mSetProvider = setProvider;
            mPresetProvider = presetProvider;
            mTagProvider = tagProvider;
            mLog = log;
        }

        public IEnumerable<PresetList> GetLists() {
            var retValue = new List<PresetList>();

            mLibraryProvider.SelectLibraries( list => retValue.AddRange( from l in list select new LibraryPresetList( l, GetPresets ))).IfLeft( ex => LogException( "SelectLibraries", ex ));
            mSetProvider.SelectSets( list => retValue.AddRange( from s in list select new SetPresetList( s, GetPresets ))).IfLeft( ex => LogException( "SelectSets", ex ));
            mTagProvider.SelectTags( list => retValue.AddRange( from t in list select new TagPresetList( t, GetPresets ))).IfLeft( ex => LogException( "SelectTags", ex ));

            retValue.Add( new GlobalPresetList( "Unrated Presets", PresetListType.Unrated, GetUnratedPresets ));
            retValue.Add( new GlobalPresetList( "Don't Play", PresetListType.DoNotPlay, GetDoNotPlayPresets ));
            retValue.Add( new GlobalPresetList( "All Presets", PresetListType.AllPresets, GetAllPresets ));
            retValue.Add( new GlobalPresetList( "Duplicated Presets", PresetListType.Duplicated, GetDuplicatedPresets ));

            return from p in retValue orderby p.Name select p;
        }

        public IEnumerable<Preset> GetPresets( PresetListType ofType, ObjectId id ) {
            var retValue = new List<Preset>() as IEnumerable<Preset>;

            switch( ofType ) {
                case PresetListType.Library:
                    retValue = GetLibraryPresets( id );
                    break;

                case PresetListType.Preset:
                    retValue = GetPreset( id );
                    break;

                case PresetListType.Set:
                    retValue = GetSetPresets( id );
                    break;

                case PresetListType.Tag:
                    retValue = GetTagPresets( id );
                    break;

                case PresetListType.DoNotPlay:
                    retValue = GetDoNotPlayPresets();
                    break;

                case PresetListType.Unrated:
                    retValue = GetUnratedPresets();
                    break;

                case PresetListType.AllPresets:
                    retValue = GetAllPresets();
                    break;
            }

            return retValue.DistinctBy( p => p.Name );
        }

        private IEnumerable<Preset> GetLibraryPresets( ObjectId id ) {
            var retValue = new List<Preset>() as IEnumerable<Preset>;

            mLibraryProvider.GetLibraryById( id ).IfRight( opt => opt.Do( lib => retValue = GetPresets( lib )));

            return retValue;
        }

        private IEnumerable<Preset> GetSetPresets( ObjectId id ) {
            var retValue = new List<Preset>() as IEnumerable<Preset>;

            mSetProvider.GetSetById( id ).IfRight( opt => opt.Do( set => retValue = GetPresets( set )));

            return retValue;
        }

        private IEnumerable<Preset> GetTagPresets( ObjectId id ) {
            var retValue = new List<Preset>() as IEnumerable<Preset>;

            mTagProvider.GetTagById( id ).IfRight( opt => opt.Do( tag => retValue = GetPresets( tag )));

            return retValue;
        }

        private IEnumerable<Preset> GetPreset( ObjectId id ) {
            var retValue = new List<Preset>();

            mPresetProvider.GetPresetById( id ).IfRight( opt => opt.Do( preset => retValue.Add( preset )));

            return retValue;
        }

        private IEnumerable<Preset> GetPresets( PresetLibrary forLibrary ) {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( forLibrary, list => retValue.AddRange( list )).IfLeft( ex => LogException( "SelectPresets", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetPresets( PresetSet forSet ) {
            var retValue = new List<Preset>();

            if( forSet.Qualifiers.Any()) {
                mSetProvider.GetPresetList( forSet, list => retValue.AddRange( list )).IfLeft( ex => LogException( "GetPresetList", ex ));
            }

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetPresets( PresetTag forTag ) {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( forTag, list => retValue.AddRange( list )).IfLeft( ex => LogException( "SelectPresets (forTag)", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetUnratedPresets() {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( list => retValue.AddRange( from p in list where !p.IsFavorite && p.Rating == PresetRating.UnRatedValue && !p.Tags.Any() select p ))
                .IfLeft( ex => LogException( "SelectPresets (unrated)", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetDoNotPlayPresets() {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( list => retValue.AddRange( from p in list where p.Rating == PresetRating.DoNotPlayValue select p ))
                .IfLeft( ex => LogException( "SelectPresets (do not play)", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetDuplicatedPresets() {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( list => retValue.AddRange( from p in list where p.IsDuplicate select p ))
                .IfLeft( ex => LogException( "SelectPresets (duplicates)", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        public IEnumerable<Preset> GetAllPresets() {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( list => retValue.AddRange( from p in list where p.Rating != PresetRating.DoNotPlayValue select p ))
                .IfLeft( ex => LogException( "SelectPresets (AllPresets)", ex ));

            return retValue.DistinctBy( p => p.Name );
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( $"PresetListProvider:{message}", ex );
        }
    }
}
