using System;

namespace Noise.Infrastructure {
	public interface ILog {
		void	LogException( string message, Exception ex );
		void	LogException( Exception ex );

		void	LogMessage( string message );
		void	LogInfo( string message );
	}
}
