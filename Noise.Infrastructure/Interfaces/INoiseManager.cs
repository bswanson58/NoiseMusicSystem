namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();
		bool			IsInitialized { get; }

		void			Shutdown();

		IDataProvider	DataProvider { get; }
		IPlayQueue		PlayQueue { get; }
		IPlayHistory	PlayHistory { get; }
		IPlayController	PlayController { get; }
	}
}
