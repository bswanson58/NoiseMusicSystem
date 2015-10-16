using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Logging {
	public interface ILogPlayQueue {
		void	AddedTrack( PlayQueueTrack track );
		void	RemovedTrack( PlayQueueTrack track );
		void	ClearedQueue();
		void	StatusChanged( PlayQueueTrack track );

		void	StrategySet( ePlayStrategy strategy, IPlayStrategyParameters parameters );
		void	StrategySet( ePlayExhaustedStrategy strategy, IPlayStrategyParameters parameters );

		void	LogQueueException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
