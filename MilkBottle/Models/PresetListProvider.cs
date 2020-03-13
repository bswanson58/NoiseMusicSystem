﻿using System;
using System.Collections.Generic;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MoreLinq;

namespace MilkBottle.Models {
    class PresetListProvider : IPresetListProvider {
        private readonly IPresetLibraryProvider mLibraryProvider;
        private readonly IPresetSetProvider     mSetProvider;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPlatformLog           mLog;

        public PresetListProvider( IPresetLibraryProvider libraryProvider, IPresetSetProvider setProvider, IPresetProvider presetProvider, IPlatformLog log ) {
            mLibraryProvider = libraryProvider;
            mSetProvider = setProvider;
            mPresetProvider = presetProvider;
            mLog = log;
        }

        public IEnumerable<PresetList> GetLists() {
            var retValue = new List<PresetList>();

            mLibraryProvider.SelectLibraries( list => retValue.AddRange( from l in list select new LibraryPresetList( l, GetPresets ))).IfLeft( ex => LogException( "SelectLibraries", ex ));
            mSetProvider.SelectSets( list => retValue.AddRange( from s in list select new SetPresetList( s, GetPresets ))).IfLeft( ex => LogException( "SelectSets", ex ));

            return from p in retValue orderby p.Name select p;
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

        private void LogException( string message, Exception ex ) {
            mLog.LogException( $"PresetListProvider:{message}", ex );
        }
    }
}
