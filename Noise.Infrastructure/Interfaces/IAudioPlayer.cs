using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IAudioPlayer {
		int			OpenFile( StorageFile file );
		void		CloseFile( int channel );

		void		Play( int channel );
		void		Fade( int channel );
		void		Pause( int channel );
		void		Stop( int channel );
		TimeSpan	PlayPosition( int channel );

		float		Volume { get; set; }
	}
}
