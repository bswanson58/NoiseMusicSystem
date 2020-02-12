using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MilkBottle.Properties;

namespace MilkBottle.Models {
    class PresetLibrarian : IPresetLibrarian {
        private readonly string[]       mSupportPresetExtensions = { "milk" };

        private readonly IEnvironment   mEnvironment;
        private readonly IPreferences   mPreferences;

        private readonly Dictionary<string, LibrarySet> mLibraries;

        public PresetLibrarian( IEnvironment environment, IPreferences preferences ) {
            mEnvironment = environment;
            mPreferences = preferences;

            mLibraries = new Dictionary<string, LibrarySet>();

            LoadLibrary();
        }

        private void LoadLibrary() {
            var libraryPath = mEnvironment.MilkLibraryFolder();

            if( Directory.Exists( libraryPath )) {
                foreach( var directory in Directory.GetDirectories( libraryPath )) {
                    var libraryName = Path.GetFileName( directory );

                    LoadLibrarySet( libraryName, directory );
                }
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
