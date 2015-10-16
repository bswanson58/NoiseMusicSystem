using System;
using System.IO;
using Noise.Infrastructure.Interfaces;
using Serilog;

namespace Noise.AppSupport.Logging {
	public class SeriLogAdapter : IPlatformLog {
		private readonly ILogger	mLog;

		public SeriLogAdapter( INoiseEnvironment environment ) {
			var logFile = Path.Combine( environment.LogFileDirectory(), environment.ApplicationName() + " log - {Date}.log" );

			mLog = new LoggerConfiguration()
				.Enrich.WithProcessId()
				.WriteTo.RollingFile( logFile, outputTemplate:"{Timestamp:MM-dd-yy HH:mm:ss.ffff} [{ProcessId}] [{Level}] {Message}{NewLine}{Exception}",
									  fileSizeLimitBytes:8192 * 1024,	retainedFileCountLimit:10 )
#if DEBUG
				.WriteTo.ColoredConsole()
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
