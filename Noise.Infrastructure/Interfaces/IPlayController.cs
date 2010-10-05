using System;
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
		double			PanPosition {get; set; }
		double			PlaySpeed { get; set; }

		double			LeftLevel { get; }
		double			RightLevel { get; }

		bool			IsFavorite {get; set; }
		Int16			Rating { get; set; }

		PlayQueueTrack	CurrentTrack { get; }
		TimeSpan		TrackTime { get; }
		void			ToggleTimeDisplay();
		long			PlayPosition { get; set; }
		long			TrackEndPosition { get; }
	}
}
