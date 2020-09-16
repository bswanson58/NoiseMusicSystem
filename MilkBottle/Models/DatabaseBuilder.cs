using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MoreLinq;

namespace MilkBottle.Models {
    class DatabaseBuilder : IDatabaseBuilder {
        private readonly IEnvironment           mEnvironment;
        private readonly IPresetProvider        mPresetProvider;
        private readonly IPresetLibraryProvider mLibraryProvider;
        private readonly IPlatformLog           mLog;

        public DatabaseBuilder( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider, IEnvironment environment, IPlatformLog log ) {
            mEnvironment = environment;
            mPresetProvider = presetProvider;
            mLibraryProvider = libraryProvider;
            mLog = log;
        }

        public Task<bool> ReconcileDatabase() {
            return Task.Run( ReconcileLibraries );
        }

        private bool ReconcileLibraries() {
            var retValue = false;

            try {
                var root = mEnvironment.MilkLibraryFolder();

                if( Directory.Exists( root )) {
                    // Traverse the folder tree.
                    ReconcileLibrary( root );

                    // remove any presets without parent directories.
                    RemoveOrphans();

                    // handle any duplicates
                    HandleDuplicates();

                    retValue = true;
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "DatabaseBuilder.ReconcileLibraries", ex );
            }

            return retValue;
        }

        private void ReconcileLibrary( string root ) {
            var directories = new List<string>();
            var libraries = new List<PresetLibrary>();

            if( Directory.Exists( root )) {
                directories = Directory.EnumerateDirectories( root ).ToList();
            }
                
            mLibraryProvider.SelectLibraries( list => libraries.AddRange( from l in list select l ));

            // add any new directories as libraries
            var missingLibraries = directories.Where( d => !libraries.Any( l => l.Name.Equals( Path.GetFileName( d )))).ToList();
            missingLibraries.ForEach( l => mLibraryProvider.Insert( new PresetLibrary( Path.GetFileName( l ), l ))
                                                .IfLeft( ex => LogException( "LibraryProvider.Insert", ex )));

            // find any deleted directories
            var missingDirectories = libraries.Where( l => !directories.Any( d => d.Equals( l.Location ))).ToList();

            // reconcile the presets in each library
            libraries.Clear();
            mLibraryProvider.SelectLibraries( list => libraries.AddRange( from l in list select l ));
            libraries.ForEach( l => ReconcileFolder( l.Location, l ));

            // remove any missing directories
            missingDirectories.ForEach( p => mLibraryProvider.Delete( p ).IfLeft( ex => LogException( "LibraryProvider.Delete", ex )));
        }

        private void ReconcileFolder( string path, PresetLibrary forLibrary ) {
            var directories = new List<string>();

            if( Directory.Exists( path )) {
                directories = Directory.EnumerateDirectories( path ).ToList();
            }

            ReconcilePresets( path, forLibrary );

            directories.ForEach( d => ReconcileFolder( d, forLibrary ));
        }

        private void ReconcilePresets( string path, PresetLibrary forLibrary ) {
            var files = new List<string>();
            var presets = new List<Preset>();

            mPresetProvider.SelectPresets( forLibrary, list => presets.AddRange( from p in list where Path.GetDirectoryName( p.Location )?.Equals( path ) == true select p ));
            if( Directory.Exists( path )) {
                files.AddRange( from f in Directory.EnumerateFiles( path ) where IsPresetFile( f ) select f );
            }

            // add any new presets found
            var missingPresets = files.Where( f => !presets.Any( p => p.ParentLibrary.Id.Equals( forLibrary.Id ) && p.Name.Equals( Path.GetFileName( f )))).ToList();
            missingPresets.ForEach( file => AddPreset( file, forLibrary ));

            // remove any presets not found
            var missingFiles = presets.Where( p => !files.Any( f => f.Equals( p.Location ))).ToList();
            missingFiles.ForEach( p => mPresetProvider.Delete( p ).IfLeft( ex => LogException( "PresetProvider.Delete", ex )));
        }

        private void AddPreset( string path, PresetLibrary library ) {
            var preset = new Preset( Path.GetFileName( path ), path, library );
            var pathFromLibrary = Path.GetDirectoryName( path )?.Replace( library.Location, String.Empty );

            if(!String.IsNullOrWhiteSpace( pathFromLibrary )) {
                var categories = from p in pathFromLibrary.Split( Path.DirectorySeparatorChar ) where !String.IsNullOrWhiteSpace( p ) select p;

                preset = preset.WithCategories( categories );
            }

            mPresetProvider.Insert( preset ).IfLeft( ex => LogException( "PresetProvider.Insert", ex ));
        }

        private bool IsPresetFile( string path ) {
            return Path.GetExtension( path ).ToLower().Equals( ".milk" );
        }

        private void RemoveOrphans() {
            var orphanedPresets = new List<Preset>();

            mPresetProvider.SelectPresets( list => orphanedPresets.AddRange( from p in list where( p.ParentLibrary == null || !File.Exists( p.Location )) select p ))
                .IfLeft( ex => LogException( "RemoveOrphans.SelectPresets", ex ));

            orphanedPresets.ForEach( p => mPresetProvider.Delete( p ).IfLeft( ex => LogException( "PresetProvider.Delete (orphaned preset)", ex )));
        }

        private void HandleDuplicates() {
            var presets = new List<Preset>();

            mPresetProvider.SelectPresets( list => presets.AddRange( list )).IfLeft( ex => LogException( "HandleDuplicates.SelectPresets", ex ) );

            var duplicates = from preset in presets 
                group preset by preset.Name into presetGroup 
                where ( presetGroup.Count() > 1 ) && presetGroup.Any( p => !p.IsDuplicate )
                select presetGroup;

            duplicates.ForEach( list => HandleDuplicates( list.ToList()));
        }

        private void HandleDuplicates( List<Preset> duplicateGroup ) {
            var isRated = duplicateGroup.FirstOrDefault( p => p.IsFavorite || p.Rating != PresetRating.UnRatedValue );

            if( isRated != null ) {
                var isFavorite = isRated.IsFavorite;
                var rating = isRated.Rating;
                var tags = isRated.Tags;

                duplicateGroup.ForEach( p => {
                    var updatedPreset = p.WithFavorite( isFavorite ).WithRating( rating ).WithTags( tags ).WithDuplicate( true );

                    mPresetProvider.Update( updatedPreset ).IfLeft( ex => LogException( "HandleDuplicates.Update", ex ));
                });
            }
            else {
                duplicateGroup.ForEach( p => {
                    var updatedPreset = p.WithDuplicate( true );

                    mPresetProvider.Update( updatedPreset ).IfLeft( ex => LogException( "HandleDuplicates.Update", ex ));
                });
            }
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex  );
        }
    }
}
