using System;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseLog {
		void	LogException( string message, Exception exception );
		void	LogException( string message, Exception exception, string methodName );

		void	LogMessage( string message );
		void	LogMessage( string message, string methodName );
	}
}
