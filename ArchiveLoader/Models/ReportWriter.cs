using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class ReportWriter : IReportWriter {
        private readonly IPreferences   mPreferences;
        private readonly IPlatformLog   mLog;

        public ReportWriter( IPreferences preferences, IPlatformLog log ) {
            mPreferences = preferences;
            mLog = log;
        }

        public void CreateReport( string volumeName, IEnumerable<CompletedProcessItem> items ) {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();
            var reportFile = Path.ChangeExtension(Path.Combine(preferences.ReportDirectory, volumeName), ".log");
            var reportTitle = $"Processing report for archive volume: {volumeName}";

            if ( volumeName.Contains( Path.VolumeSeparatorChar.ToString())) {
                reportFile = Path.ChangeExtension( Path.Combine( preferences.ReportDirectory, Path.GetFileNameWithoutExtension( volumeName )), ".log" );
            }

            Task.Run(() => { WriteReport( reportFile, reportTitle, items );});
        }

        private void WriteReport( string toFile, string title, IEnumerable<CompletedProcessItem> items ) {
            try {
                var directory = Path.GetDirectoryName( toFile );

                if((!String.IsNullOrWhiteSpace(directory )) &&
                   (!Directory.Exists( directory ))) {
                    Directory.CreateDirectory( directory );
                }

                using( var outputFile = new StreamWriter( toFile )) {
                    var filesCopied = 0;
                    var errors = 0;

                    outputFile.WriteLine( title );
                    outputFile.WriteLine();

                    foreach( var item in items ) {
                        WriteItem( outputFile, item );

                        filesCopied++;
                        errors += item.Errors;
                    }

                    outputFile.WriteLine( $"Summary: {filesCopied} files copied, {errors} errors." );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"WriteReport for to file: {toFile}", ex );
            }
        }

        private void WriteItem( StreamWriter writer, CompletedProcessItem item ) {
            writer.WriteLine( item.Name + new string( ' ', Math.Max( 2, 80 - ( item.Name.Length + item.ProcessNames.Length ))) + item.ProcessNames );
            writer.WriteLine( "     " + item.FileName );

            if(!string.IsNullOrWhiteSpace( item.Output )) {
                writer.Write( item.Output );
            }

            writer.WriteLine( "-----" );
        }
    }
}
