using System;
using System.IO;

namespace Noise.RemoteClient.Droid.Support {
    public static class ExceptionFileWriter {
        static string FilePath {
            get {
                const string errorFilename = "Fatal.log";
                var libraryPath = Environment.GetFolderPath( Environment.SpecialFolder.Personal );
                
                return Path.Combine( libraryPath, errorFilename );
            }
        }

        public static void LogUnhandledException( this Exception exception ) {
            try {
                var errorMessage =
                    $"Time: {DateTime.Now}\r\nError: Unhandled Exception\r\n{( string.IsNullOrEmpty( exception.StackTrace ) ? exception.ToString() : exception.StackTrace )}\n\n";
                File.WriteAllText( FilePath, errorMessage );
            }
            catch( Exception ) {
                // just suppress any error logging exceptions
            }
        }
    }
}