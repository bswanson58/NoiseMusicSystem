using System;
using System.IO;
using Noise.RemoteClient.Interfaces;
using Serilog;

namespace Noise.RemoteClient.Platform {
    class SeriLogAdapter : IPlatformLog {
        private readonly IClientEnvironment mEnvironment;
        private ILogger	                    mLog;

        public SeriLogAdapter( IClientEnvironment environment ) {
            mEnvironment = environment;
        }

        private ILogger Log {
            get {
                if( mLog == null ) {
                    mLog = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .WriteTo.File( Path.Combine( mEnvironment.LogDirectory, "log - .log" ), 
                                        rollingInterval: RollingInterval.Day, retainedFileCountLimit:10, fileSizeLimitBytes:8192 * 1024,
                                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}{NewLine}" )
                        .CreateLogger();
                }

                return mLog;
            }
        }

        public void LogException( string message, Exception ex ) {
            Log.Error( ex, message );
        }

        public void LogMessage( string message ) {
            Log.Information( message );
        }
    }
}
