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

		PlayQueueTrack	CurrentTrack { get; }
		TimeSpan		TrackTime { get; }
		long			PlayPosition { get; set; }
		long			TrackEndPosition { get; }
	}
}
