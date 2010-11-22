using System;
using Noise.Infrastructure.Dto;

namespace Noise.Core.MediaPlayer {
	public interface IAudioPlayer {
		int					OpenFile( StorageFile file );
		int					OpenFile( StorageFile file, float gainAdjustment );
		int					OpenStream( DbInternetStream stream );
		void				CloseChannel( int channel );

		void				Play( int channel );
		void				FadeAndPause( int channel );
		void				FadeAndStop( int channel );
		void				Pause( int channel );
		void				Stop( int channel );
		TimeSpan			GetLength( int channel );
		TimeSpan			GetPlayPosition( int channel );
		void				SetPlayPosition( int channel, TimeSpan position );
		ePlaybackStatus		GetChannelStatus( int channel );
		double				GetPercentPlayed( int channel );

		AudioLevels			GetSampleLevels( int channel );

		float				Volume { get; set; }
		float				PreampVolume { get; set; }
		float				Pan { get; set; }
		float				PlaySpeed { get; set; }

		ParametricEqualizer ParametricEq { get; set; }
		bool				EqEnabled { get; set; }
		void				AdjustEq( long bandId, float gain );
	}
}
