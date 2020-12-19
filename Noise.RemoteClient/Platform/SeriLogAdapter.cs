using System;
using System.IO;
using Noise.RemoteClient.Interfaces;
using Serilog;
using Xamarin.Essentials;

namespace Noise.RemoteClient.Platform {
    class SeriLogAdapter : IPlatformLog {
        private readonly ILogger	mLog;

        public SeriLogAdapter() {
//            var logFile = Path.Combine( environment.LogFileDirectory(), environment.ApplicationName() + " log - {Date}.log" );

            mLog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(FileSystem.AppDataDirectory, "Logs", "Log.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}" )
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
