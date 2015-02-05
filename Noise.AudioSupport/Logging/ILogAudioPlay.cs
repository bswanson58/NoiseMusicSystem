using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.AudioSupport.Logging {
	public interface ILogAudioPlay {
		void	LogPluginLoaded( string plugin );
		void	LogPluginLoadFailed( string plugin, int errorCode );

		void	LogSetOutputDevice( AudioDevice device );

		void	LogChannelOpen( int channel, string channelName );
		void	LogChannelPlay( int channel );
		void	LogChannelStatus( AudioChannelStatus status );
		void	LogChannelClose( int channel );

		void	LogErrorCode( string message, int errorCode, [CallerMemberName] string callerName = "" );
		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
