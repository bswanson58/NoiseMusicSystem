using System;
using System.IO;
using Serilog;
using TuneArchiver.Interfaces;

namespace TuneArchiver.Logging {
    public class SeriLogAdapter : IPlatformLog {
        private readonly ILogger	mLog;

        public SeriLogAdapter( IEnvironment environment ) {
            var logFile = Path.Combine( environment.LogFileDirectory(), environment.ApplicationName() + " log - {Date}.log" );

            mLog = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .WriteTo.RollingFile( logFile, outputTemplate:"{Timestamp:MM-dd-yy HH:mm:ss.ffff} [{ProcessId}] [{Level}] {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes:8192 * 1024,	retainedFileCountLimit:10 )
#if DEBUG
                .WriteTo.Console()
#endif
                .CreateLogger();
        }

        public void LogException( string message, Exception ex ) {
            mLog.Error( ex, message );
        }

        public void LogMessage( string message ) {
            mLog.Information( message );
        }
    }
}
