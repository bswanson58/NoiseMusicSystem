namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();
		bool			IsInitialized { get; }

		void			Shutdown();

		IDataProvider	DataProvider { get; }
		ISearchProvider	SearchProvider { get; }
		ILibraryBuilder	LibraryBuilder { get; }
		IPlayQueue		PlayQueue { get; }
		IPlayHistory	PlayHistory { get; }
		IPlayListMgr	PlayListMgr { get; }
		IPlayController	PlayController { get; }
	}
}
