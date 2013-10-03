using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayState {
		StoppedEmptyQueue,
		Stopped,
		Stopping,
		StartPlaying,
		Playing,
		PlayNext,
		PlayPrevious,
		Paused,
		Pausing,
		Resuming,
		ExternalPlay
	}

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

		bool			ReplayGainEnable { get; set; }

		double			LeftLevel { get; }
		double			RightLevel { get; }
		BitmapSource	GetSpectrumImage( int height, int width, Color baseColor, Color peakColor, Color peakHoldColor );

		bool			IsFavorite {get; set; }
		Int16			Rating { get; set; }

		PlayQueueTrack	CurrentTrack { get; }
		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		TimeSpan		TrackTime { get; }
		void			ToggleTimeDisplay();
		long			PlayPosition { get; set; }
		double			PlayPositionPercentage { get; }
		long			TrackEndPosition { get; }

		IObservable<ePlayState>	PlayStateChange { get; }
	}
}
