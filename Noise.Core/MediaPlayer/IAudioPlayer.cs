using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;

namespace Noise.Core.MediaPlayer {
	public interface IAudioPlayer {
		int					OpenFile( string filePath );
		int					OpenFile( string filePath, float gainAdjustment );
		int					OpenStream( DbInternetStream stream );
		void				CloseChannel( int channel );

		void				Play( int channel );
		void				FadeAndPause( int channel );
		void				FadeAndStop( int channel );
		void				Pause( int channel );
		void				Stop( int channel );
		void				QueueNextChannel( int channel );

		TimeSpan			GetLength( int channel );
		TimeSpan			GetPlayPosition( int channel );
		void				SetPlayPosition( int channel, TimeSpan position );
		ePlaybackStatus		GetChannelStatus( int channel );
		double				GetPercentPlayed( int channel );

		float				Volume { get; set; }
		float				PreampVolume { get; set; }
		bool				Mute { get; set; }
		float				Pan { get; set; }
		float				PlaySpeed { get; set; }

		ParametricEqualizer ParametricEq { get; set; }
		bool				EqEnabled { get; set; }
		void				AdjustEq( long bandId, float gain );

		bool				StereoEnhancerEnable { get; set; }
		double				StereoEnhancerWidth { get; set; }
		double				StereoEnhancerWetDry { get; set; }

		bool				SoftSaturationEnable { get; set; }
		double				SoftSaturationFactor { get; set; }
		double				SoftSaturationDepth { get; set; }

		bool				ReverbEnable { get; set; }
		float				ReverbLevel { get; set; }
		int					ReverbDelay { get; set; }

		IObservable<ChannelStatusArgs>	ChannelStatusChange { get; }
		IObservable<AudioLevels>		AudioLevelsChange { get; }
		IObservable<StreamInfo>			AudioStreamInfoChange { get; }

		BitmapSource		GetSpectrumImage( int channel, int height, int width, Color baseColor, Color peakColor, Color peakHoldColor );
	}
}
