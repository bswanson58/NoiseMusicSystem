using System;
using System.Runtime.CompilerServices;

namespace Noise.Infrastructure.Interfaces {
	public interface IApplicationLog {
		void	ApplicationStarting();
		void	ApplicationExiting();

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
