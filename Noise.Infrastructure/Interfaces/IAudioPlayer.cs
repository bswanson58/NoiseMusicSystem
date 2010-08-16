﻿using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayingChannelStatus {
		Stopped,
		Playing,
		Paused,
		Unknown
	}

	public interface IAudioPlayer {
		int						OpenFile( StorageFile file );
		int						OpenStream( string url );
		void					CloseChannel( int channel );

		void					Play( int channel );
		void					FadeAndPause( int channel );
		void					FadeAndStop( int channel );
		void					Pause( int channel );
		void					Stop( int channel );
		TimeSpan				GetLength( int channel );
		TimeSpan				GetPlayPosition( int channel );
		void					SetPlayPosition( int channel, TimeSpan position );
		ePlayingChannelStatus	GetChannelStatus( int channel );
		double					GetPercentPlayed( int channel );

		AudioLevels				GetSampleLevels( int channel );

		float		Volume { get; set; }
		float		Pan { get; set; }
		float		PlaySpeed { get; set; }
	}
}
