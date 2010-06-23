﻿namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();

		IAudioPlayer	AudioPlayer { get; }
		IDataProvider	DataProvider { get; }
		IPlayQueue		PlayQueue { get; }

		void			Explore();
	}
}
