using System;
using System.Runtime.CompilerServices;

namespace Noise.UI.Logging {
	public interface IUiLog {
		void	LogMessage( string message, [CallerMemberName] string callerName = "" );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
