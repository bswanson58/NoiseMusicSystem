using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Logging {
	public interface ILogPlayStrategy {
		void	LogTrackQueued( ePlayExhaustedStrategy strategy, DbTrack track );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
