using System;
using System.Runtime.CompilerServices;
using Noise.Core.PlaySupport;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Logging {
	internal interface ILogPlayState {
		void	PlayStateTrigger( eStateTriggers trigger );
		void	PlayStateSet( ePlayState state );
		void	PlaybackStatusChanged( ePlaybackStatus status );

		void	LogPlayStateException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
