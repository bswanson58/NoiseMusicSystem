using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MilkBottle.Logging {
    class LoggingSink {
        public  ILogEventSink   Sink { get; }
        public  LogEventLevel   EventLevel { get; }

        public LoggingSink( ILogEventSink sink, LogEventLevel forMinimumEventLevel ) {
            Sink = sink;
            EventLevel = forMinimumEventLevel;
        }
    }

    public class SeriLogAdapter : IPlatformLog {
        private readonly IEnvironment           mEnvironment;
        private readonly List<LoggingSink>      mAdditionalSinks;
        private ILogger	                        mLog;

        public SeriLogAdapter( IEnvironment environment ) {
            mEnvironment = environment;

            mAdditionalSinks = new List<LoggingSink>();
        }

        public void AddLoggingSink( ILogEventSink sink, LogEventLevel forMinimumEventLevel = LogEventLevel.Information ) {
            mAdditionalSinks.Add( new LoggingSink( sink, forMinimumEventLevel ));
        }

        private void CreateLog() {
            var logFile = Path.Combine( mEnvironment.LogFileDirectory(), mEnvironment.ApplicationName() + " log - {Date}.log" );

            var configuration = new LoggerConfiguration()
                .Enrich.WithProcessId()
                .WriteTo.RollingFile( logFile, outputTemplate:"{Timestamp:MM-dd-yy HH:mm:ss.ffff} [{ProcessId}] [{Level}] {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes:8192 * 1024,	retainedFileCountLimit:10 );
#if DEBUG
                configuration.WriteTo.Console();
#endif

            if( mAdditionalSinks.Any()) {
                mAdditionalSinks.ForEach( sink => configuration.WriteTo.Sink( sink.Sink, sink.EventLevel ));
            }
            
            mLog = configuration.CreateLogger();
        }

        private ILogger Log {
            get {
                if( mLog == null ) {
                    CreateLog();
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
