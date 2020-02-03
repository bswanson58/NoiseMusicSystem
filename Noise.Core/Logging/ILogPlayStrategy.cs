using System;
using System.Runtime.CompilerServices;

namespace Noise.Core.Logging {
	public interface ILogPlayStrategy {
		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
