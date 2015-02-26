using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	public class LogLibraryConfiguration : BaseLogger, ILogLibraryConfiguration {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library";
		private const string	cPhaseName = "Configuration";

		public LogLibraryConfiguration( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogConfigurationMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LibraryOpened( LibraryConfiguration configuration ) {
			LogOnCondition( mPreferences.LibraryConfiguration || mPreferences.BasicActivity, () => LogConfigurationMessage( "Opened {0}", configuration ));
		}

		public void LibraryClosed( LibraryConfiguration configuration ) {
			LogOnCondition( mPreferences.LibraryConfiguration, () => LogConfigurationMessage( "Closed {0}", configuration ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
