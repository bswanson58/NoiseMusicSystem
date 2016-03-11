using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Speech.Logging {
	internal class SpeechLogger : BaseLogger, ILogSpeech {
		private readonly LoggingPreferences mPreferences;

		private const string cModuleName	= "SpeechRecognition";
		private const string cPhaseName		= "Recognize";

		public SpeechLogger( LoggingPreferences preferences, IPlatformLog logger ) :
			base( logger ) {
			mPreferences = preferences;
		}

		private void LogSpeechMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ) );
		}
	}
}
