using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayController {
		void			Play();
		bool			CanPlay { get; }
		void			Pause();
		bool			CanPause { get; }
		void			Stop();
		bool			CanStop { get; }
		void			PlayNextTrack();
		bool			CanPlayNextTrack { get; }
		void			PlayPreviousTrack();
		bool			CanPlayPreviousTrack { get; }

		double			Volume { get; set; }
		bool			Mute { get; set; }
		double			PreampVolume { get; set; }
		bool			ReplayGainEnable { get; set; }

		double			PanPosition {get; set; }
		double			PlaySpeed { get; set; }

		IEqManager			EqManager { get; }
		ParametricEqualizer	CurrentEq { get; set; }
		bool				EqEnabled { get; set; }
		void				SetEqValue( long bandId, float gain );

		bool			StereoEnhancerEnable { get; set; }
		double			StereoEnhancerWidth { get; set; }
		double			StereoEnhancerWetDry { get; set; }

		double			LeftLevel { get; }
		double			RightLevel { get; }

		bool			IsFavorite {get; set; }
		Int16			Rating { get; set; }

		PlayQueueTrack	CurrentTrack { get; }
		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		TimeSpan		TrackTime { get; }
		void			ToggleTimeDisplay();
		long			PlayPosition { get; set; }
		long			TrackEndPosition { get; }

		BitmapSource	GetSpectrumImage( int height, int width, Color baseColor, Color peakColor, Color peakHoldColor );
	}
}
