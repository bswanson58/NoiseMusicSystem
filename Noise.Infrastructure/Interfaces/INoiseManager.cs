namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();
		void			Shutdown();

		IDataProvider	DataProvider { get; }
		IPlayQueue		PlayQueue { get; }
		IPlayHistory	PlayHistory { get; }
		IPlayController	PlayController { get; }
	}
}
