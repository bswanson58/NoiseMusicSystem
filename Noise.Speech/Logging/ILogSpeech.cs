using System;
using System.Runtime.CompilerServices;

namespace Noise.Speech.Logging {
	internal interface ILogSpeech {
		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
