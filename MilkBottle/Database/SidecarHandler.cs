using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;

namespace MilkBottle.Database {
    class SidecarHandler : ISidecarHandler {
        private readonly ITagProvider           mTagProvider;
        private readonly IFileWriter            mFileWriter;
        private readonly IPlatformLog           mLog;
        private readonly IApplicationConstants  mApplicationConstants;

        public SidecarHandler( ITagProvider tagProvider, IFileWriter fileWriter, IPlatformLog log, IApplicationConstants constants ) {
            mTagProvider = tagProvider;
            mFileWriter = fileWriter;
            mLog = log;
            mApplicationConstants = constants;
        }

        public void SaveSidecar( Preset forPreset ) {
            try {
                var sidecar = new PresetSidecar( forPreset );
                var path = Path.ChangeExtension( forPreset.Location, mApplicationConstants.PresetSidecarExtension );

                mFileWriter.Write( path, sidecar );
            }
            catch( Exception ex ) {
                mLog.LogException( "SaveSidecar", ex );
            }
        }

        public Preset LoadSidecar( Preset forPreset ) {
            var preset = forPreset;

            try {
                var path = Path.ChangeExtension( forPreset.Location, mApplicationConstants.PresetSidecarExtension );

                if( File.Exists( path )) {
                    var sidecar = mFileWriter.Read<PresetSidecar>( path );

                    if( sidecar != null ) {
                        preset = preset.WithFavorite( sidecar.IsFavorite ).WithRating( sidecar.Rating ).WithTags( new PresetTag[]{ });

                        if( sidecar.Tags.Any()) {
                            var tagList = new List<PresetTag>();
                        
                            mTagProvider.SelectTags( tags => tagList.AddRange( tags )).IfLeft( ex => mLog.LogException( "LoadSidecar:SelectTags", ex ));

                            sidecar.Tags.ForEach( sidecarTag => {
                                var tag = tagList.FirstOrDefault( t => t.Name.Equals( sidecarTag ));

                                if( tag != null ) {
                                    preset = preset.WithTagState( tag, true );
                                }
                                else {
                                    tag = new PresetTag( sidecarTag );

                                    mTagProvider.Insert( tag ).IfLeft( ex => mLog.LogException( "LoadSidecar:Insert Tag", ex ));
                                    preset = preset.WithTagState( tag, true );
                                    tagList.Add( tag );
                                }
                            });
                        }
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "LoadSidecar", ex );
            }

            return preset;
        }
    }
}
