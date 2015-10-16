using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.AudioSupport.Logging {
	internal class LogAudioPlay : BaseLogger, ILogAudioPlay {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Audio";
		private const string	cPhaseName = "Playback";

		public LogAudioPlay( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogAudioMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogPluginLoaded( string plugin ) {
			LogOnCondition( mPreferences.AudioPlayback, () => LogAudioMessage( "Loaded plugin \"{0}\"", plugin ));
		}

		public void LogPluginLoadFailed( string plugin, int errorCode ) {
			LogOnCondition( mPreferences.AudioPlayback, () => LogAudioMessage( "Loading plugin failed \"{0}\", error code: {1}", plugin, errorCode ));
		}

		public void LogSetOutputDevice( AudioDevice device ) {
			LogOnCondition( mPreferences.AudioPlayback, () => LogAudioMessage( "Setting output device \"{0}\"", device ));
		}

		public void LogChannelOpen( int channel, string channelName ) {
			LogOnCondition( mPreferences.AudioSync, () => LogAudioMessage( "Opening channel {0} \"{1}\"", channel, channelName ));
		}

		public void LogChannelPlay( int channel ) {
			LogOnCondition( mPreferences.AudioSync, () => LogAudioMessage( "Playing channel {0}", channel ));
		}

		public void LogChannelStatus( AudioChannelStatus status ) {
			LogOnCondition( mPreferences.AudioSync, () => LogAudioMessage( "Status {0}", status ));
		}

		public void LogChannelClose( int channel ) {
			LogOnCondition( mPreferences.AudioSync, () => LogAudioMessage( "Closing channel {0}", channel ));
		}

		public void LogErrorCode( string message, int errorCode, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogAudioMessage( "{0}, error code: {1}, method:{2}", message, errorCode, callerName ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
