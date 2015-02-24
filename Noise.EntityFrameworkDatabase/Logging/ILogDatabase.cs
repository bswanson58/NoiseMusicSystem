using System;
using System.Runtime.CompilerServices;

namespace Noise.EntityFrameworkDatabase.Logging {
	internal interface ILogDatabase {

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
