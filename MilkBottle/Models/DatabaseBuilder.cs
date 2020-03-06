using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MilkBottle.Entities;
using MilkBottle.Interfaces;

namespace MilkBottle.Models {
    class DatabaseBuilder : IDatabaseBuilder {
        private readonly IEnvironment           mEnvironment;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetLibraryProvider mLibraryProvider;

        public DatabaseBuilder( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider, IEnvironment environment ) {
            mEnvironment = environment;
            mPresetProvider = presetProvider;
            mLibraryProvider = libraryProvider;
        }

        public Task ReconcileDatabase() {
            return Task.Run( ReconcileLibraries );
        }

        private void ReconcileLibraries() {
            var root = mEnvironment.MilkLibraryFolder();

            if( Directory.Exists( root )) {
                var directories = Directory.EnumerateDirectories( root ).ToList();
                var libraries = new List<PresetLibrary>();
                
                mLibraryProvider.SelectLibraries( list => libraries.AddRange( list ));

                // add any new directories as libraries
                var missingLibraries = directories.Where( d => !libraries.Any( l => l.Name.Equals( Path.GetFileName( d )))).ToList();
                missingLibraries.ForEach( l => mLibraryProvider.Insert( new PresetLibrary( Path.GetFileName( l ), l )).IfLeft( LogException ));

                // remove any deleted directories
                var missingDirectories = libraries.Where( l => !directories.Any( d => d.Equals( l.Location ))).ToList();
                missingDirectories.ForEach( p => mLibraryProvider.Delete( p ).IfLeft( LogException ));

                // reconcile the presets in each library
                libraries.Clear();
                mLibraryProvider.SelectLibraries( list => libraries.AddRange( list ));
                libraries.ForEach( ReconcilePresets );
            }
        }

        private void ReconcilePresets( PresetLibrary forLibrary ) {
            var files = Directory.EnumerateFiles( forLibrary.Location ).ToList();
            var presets = new List<Preset>();

            mPresetProvider.SelectPresets( forLibrary, list => presets.AddRange( list ));

            // add any new presets found
            var missingPresets = files.Where( f => !presets.Any( p => p.Library.Id.Equals( forLibrary.Id ) && p.Name.Equals( Path.GetFileName( f )))).ToList();
            missingPresets.ForEach( file => mPresetProvider.Insert( new Preset( Path.GetFileName( file ), file, forLibrary )).IfLeft( LogException ));

            // remove and presets not found
            var missingFiles = presets.Where( p => !files.Any( f => f.Equals( p.Location ))).ToList();
            missingFiles.ForEach( p => mPresetProvider.Delete( p ).IfLeft( LogException ));
        }

        private void LogException( Exception ex ) { }
    }
}
