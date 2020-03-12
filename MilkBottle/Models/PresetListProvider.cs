using System;
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

            mLibraryProvider.SelectLibraries( list => retValue.AddRange( from l in list select new LibraryPresetList( l, GetPresets )));
            mSetProvider.SelectSets( list => retValue.AddRange( from s in list select new SetPresetList( s, GetPresets )));

            return from p in retValue orderby p.Name select p;
        }

        private IEnumerable<Preset> GetPresets( PresetLibrary forLibrary ) {
            var retValue = new List<Preset>();

            mPresetProvider.SelectPresets( forLibrary, list => retValue.AddRange( list )).IfLeft( LogException );

            return retValue.DistinctBy( p => p.Name );
        }

        private IEnumerable<Preset> GetPresets( PresetSet forSet ) {
            var retValue = new List<Preset>();

            mSetProvider.GetPresetList( forSet, list => retValue.AddRange( list )).IfLeft( LogException );

            return retValue.DistinctBy( p => p.Name );
        }

        private void LogException( Exception ex ) {
            mLog.LogException( "PresetListProvider:GetPresets", ex );
        }
    }
}
