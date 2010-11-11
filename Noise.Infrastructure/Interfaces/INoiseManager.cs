namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool			Initialize();
		bool			IsInitialized { get; }

		void			StartExplorerJobs();
		void			Shutdown();

		void			ConfigurationChanged();

		IDataProvider	DataProvider { get; }
		ISearchProvider	SearchProvider { get; }
		ILibraryBuilder	LibraryBuilder { get; }
		IPlayQueue		PlayQueue { get; }
		IPlayHistory	PlayHistory { get; }
		IPlayListMgr	PlayListMgr { get; }
		IPlayController	PlayController { get; }
		ITagManager		TagManager { get; }
	}
}
