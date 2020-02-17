using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Properties;

namespace MilkBottle.Models {
    class PresetLibrarian : IPresetLibrarian {
        private readonly string[]       mSupportPresetExtensions = { "milk" };

        private readonly IEventAggregator   mEventAggregator;
        private readonly IEnvironment       mEnvironment;
        private readonly IPreferences       mPreferences;
        private readonly IPlatformLog       mLog;

        private readonly Dictionary<string, LibrarySet> mLibraries;

        public  IEnumerable<string>         AvailableLibraries => mLibraries.Keys;
        public  IEnumerable<LibrarySet>     PresetLibraries => mLibraries.Values;
        public  bool                        IsInitialized { get; private set; }

        public PresetLibrarian( IEventAggregator eventAggregator, IEnvironment environment, IPreferences preferences, IPlatformLog log ) {
            mEventAggregator = eventAggregator;
            mEnvironment = environment;
            mPreferences = preferences;
            mLog = log;

            mLibraries = new Dictionary<string, LibrarySet>();

            Initialize();
        }

        private void Initialize() {
            LoadLibrary();

            mEventAggregator.PublishOnUIThread( new Events.PresetLibraryInitialized());
        }

        public LibrarySet GetLibrary( string libraryName ) {
            var retValue = default( LibrarySet );

            if( mLibraries.ContainsKey( libraryName )) {
                retValue = mLibraries[libraryName];
            }

            return retValue;
        }

        private void LoadLibrary() {
            var libraryPath = mEnvironment.MilkLibraryFolder();

            try {
                if( Directory.Exists( libraryPath )) {
                    foreach( var directory in Directory.GetDirectories( libraryPath )) {
                        var libraryName = Path.GetFileName( directory );

                        LoadLibrarySet( libraryName, directory );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "PresetLibrarian:LoadLibrary", ex );
            }

            if( mLibraries.Any()) {
                IsInitialized = true;

                mEventAggregator.PublishOnUIThread( new Events.PresetLibraryUpdated());
            }
        }

        private void LoadLibrarySet( string name, string path ) {
            var libraryPath = ComposeLibraryName( name );
            var librarySet = mPreferences.Load<LibrarySet>( libraryPath );

            if ( String.IsNullOrWhiteSpace( librarySet.LibraryName ) ) {
                librarySet.LibraryName = name;
            }

            ReconcileLibrarySet( librarySet, path );

            if( librarySet.Presets.Any()) {
                mLibraries.Add( name, librarySet );

                mPreferences.Save( librarySet, libraryPath );
            }
        }

        private void ReconcileLibrarySet( LibrarySet set, string path ) {
            var locatedFiles = new List<MilkDropPreset>();

            foreach( var file in Directory.GetFiles( path, $"*.{mSupportPresetExtensions[0]}" )) {
                var presetName = Path.GetFileName( file );
                var preset = set.Presets.FirstOrDefault( p => p.PresetName.Equals( presetName ));

                locatedFiles.Add( preset ?? new MilkDropPreset( presetName, file ) );
            }

            set.Presets.Clear();
            set.Presets.AddRange( locatedFiles );
        }

        private string ComposeLibraryName( string name ) {
            return Path.ChangeExtension( Path.Combine( mEnvironment.MilkLibraryFolder(),  name ), ApplicationConstants.MilkLibraryExtension );
        }
    }
}
