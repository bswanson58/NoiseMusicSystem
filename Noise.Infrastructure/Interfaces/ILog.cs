using System;

namespace Noise.Infrastructure.Interfaces {
	public interface ILog {
		void	LogException( string message, Exception ex );

		void	LogMessage( string format, params object[] parameters );
		void	LogMessage( string message );

		void	LogInfo( string message );
	}
}
