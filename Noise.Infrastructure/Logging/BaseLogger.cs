using System;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Logging {
	public class BaseLogger {
		private readonly IPlatformLog	mLogger;

		protected BaseLogger( IPlatformLog logger ) {
			mLogger = logger;
		}

		protected void LogOnCondition( bool condition, Action logAction ) {
			if( condition ) {
				logAction();
			}
		}

		private string FormatModuleName( string moduleName, string phaseName ) {
			return( string.Format( "{0}:{1}", moduleName, phaseName ));
		}

		private string ComposeMessage( string moduleName, string phaseName, string message ) {
			return( FormatModuleName( moduleName, phaseName ) + " - " + message );
		}

		private void LogMessage( string format, params object[] parameters ) {
			mLogger.LogMessage( string.Format( format, parameters ));
		}

		private void LogException( string message, Exception exception, string callerName = "" ) {
			mLogger.LogException( string.Format( "Exception - {0} - {1}", message, callerName ), exception );
		}

		protected void LogMessage( string moduleName, string phaseName, string format, params object[] parameters ) {
			LogMessage( ComposeMessage( moduleName, phaseName, string.Format( format, parameters )));
		}


		protected void LogException( string moduleName, string phaseName, Exception exception, string callerName, string format, params object[] parameters ) {
			LogException( ComposeMessage( moduleName, phaseName, string.Format( format, parameters )), exception, callerName );
		}
	}
}
