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
                    var library = PresetLibrary.RootLibrary( root );

                    // Traverse the folder tree.
                    ReconcileLibrary( library );

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

        private void ReconcileLibrary( PresetLibrary root ) {
            var directories = new List<string>();
            var libraries = new List<PresetLibrary>();

            if( Directory.Exists( root.Location )) {
                directories = Directory.EnumerateDirectories( root.Location ).ToList();
            }
                
            mLibraryProvider.SelectLibraries( list => libraries.AddRange( from l in list where l.Parent.Equals( root.Id ) select l ));

            // add any new directories as libraries
            var missingLibraries = directories.Where( d => !libraries.Any( l => l.Name.Equals( Path.GetFileName( d )))).ToList();
            missingLibraries.ForEach( l => mLibraryProvider.Insert( new PresetLibrary( Path.GetFileName( l ), l, root ))
                                                .IfLeft( ex => LogException( "LibraryProvider.Insert", ex )));

            // find any deleted directories
            var missingDirectories = libraries.Where( l => !directories.Any( d => d.Equals( l.Location ))).ToList();

            // reconcile the presets in each library
            libraries.Clear();
            mLibraryProvider.SelectLibraries( list => libraries.AddRange( from l in list where l.Parent.Equals( root.Id ) select l ));
            libraries.ForEach( ReconcilePresets );

            // traverse into each folder
            libraries.ForEach( ReconcileLibrary );

            // remove any missing directories
            missingDirectories.ForEach( p => mLibraryProvider.Delete( p ).IfLeft( ex => LogException( "LibraryProvider.Delete", ex )));
        }

        private void ReconcilePresets( PresetLibrary forLibrary ) {
            var files = new List<String>();
            var presets = new List<Preset>();

            mPresetProvider.SelectPresets( forLibrary, list => presets.AddRange( list ));

            if( Directory.Exists( forLibrary.Location )) {
                files.AddRange( from f in Directory.EnumerateFiles( forLibrary.Location ) where IsPresetFile( f) select f );
            }

            // add any new presets found
            var missingPresets = files.Where( f => !presets.Any( p => p.Library.Id.Equals( forLibrary.Id ) && p.Name.Equals( Path.GetFileName( f )))).ToList();
            missingPresets.ForEach( file => mPresetProvider.Insert( new Preset( Path.GetFileName( file ), file, forLibrary ))
                                        .IfLeft( ex => LogException( "PresetProvider.Insert", ex )));

            // remove any presets not found
            var missingFiles = presets.Where( p => !files.Any( f => f.Equals( p.Location ))).ToList();
            missingFiles.ForEach( p => mPresetProvider.Delete( p ).IfLeft( ex => LogException( "PresetProvider.Delete", ex )));
        }

        private bool IsPresetFile( string path ) {
            return Path.GetExtension( path ).ToLower().Equals( ".milk" );
        }

        private void RemoveOrphans() {
            var orphanedPresets = new List<Preset>();

            mPresetProvider.SelectPresets( list => orphanedPresets.AddRange( from p in list where p.Library == null select p ))
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
