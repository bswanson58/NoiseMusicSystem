using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAudioPlayer {
		bool		OpenFile( StorageFile file );
		void		CloseFile();
		bool		IsOpen { get; }

		void		Play();
		void		Pause();
		void		Stop();

		TimeSpan	PlayPosition { get; set; }
		float		Volume { get; set; }
	}
}
