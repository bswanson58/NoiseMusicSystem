using System;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlatformLog {
		void	LogException( string message, Exception exception );
		void	LogMessage( string message );
	}
}
