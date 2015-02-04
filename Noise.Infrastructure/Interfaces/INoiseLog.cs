using System;

namespace Noise.Infrastructure.Interfaces {
	public interface INoiseLog {
		void	LogException( string message, Exception exception );

		void	LogMessage( string message );

		[Obsolete("LogMessage with format parameters is deprecated.")]
		void	LogMessage( string format, params object[] parameters );

		[Obsolete("LogInfo is deprecated, please use LogMessage instead.")]
		void	LogInfo( string message );

	}
}
