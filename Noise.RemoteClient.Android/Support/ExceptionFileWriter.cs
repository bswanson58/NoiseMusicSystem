using System;
using System.IO;

namespace Noise.RemoteClient.Droid.Support {
    public static class ExceptionFileWriter {
        private const string    cCompanyName        = "Secret Squirrel Software";
        private const string    cApplicationName    = "Noise Remote";
        private const string    cLogDirectory       = "Logs";
        private const string    cErrorFileName      = "Unhandled Exceptions.log";

        static string LogDirectory => 
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), cCompanyName, cApplicationName, cLogDirectory );

        public static void LogUnhandledException( this Exception exception ) {
            try {
                var directory = LogDirectory;
                var errorMessage =
                    $"Time: {DateTime.Now}\r\nError: Unhandled Exception\r\n{( string.IsNullOrEmpty( exception.StackTrace ) ? exception.ToString() : exception.StackTrace )}\n\n";

                if(!Directory.Exists( LogDirectory )) {
                    Directory.CreateDirectory( LogDirectory );
                }

                File.AppendAllText( Path.Combine( directory, cErrorFileName ), errorMessage );
            }
            catch( Exception ) {
                // just suppress any error logging exceptions
            }
        }
    }
}