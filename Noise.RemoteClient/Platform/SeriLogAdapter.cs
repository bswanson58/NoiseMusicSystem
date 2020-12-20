using System;
using System.IO;
using Noise.RemoteClient.Interfaces;
using Serilog;

namespace Noise.RemoteClient.Platform {
    class SeriLogAdapter : IPlatformLog {
        private readonly ILogger	mLog;

        public SeriLogAdapter() {
            string filePath = Path.Combine( "/storage/emulated/0/Android/data", "Noise", "Logs", "error.log" );

            mLog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    filePath, // Path.Combine( FileSystem.AppDataDirectory, "Logs", "Log.log" ),
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
