using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ISpeechRecognizer {
		IObservable<CommandRequest>		SpeechCommandRequest { get; } 
	}
}
