using System;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseLog {
		void	LogException( string message, Exception exception );

		void	LogMessage( string message );
	}
}
