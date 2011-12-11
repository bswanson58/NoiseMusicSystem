namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool					Initialize();
		bool					IsInitialized { get; }

		void					StartExplorerJobs();
		void					Shutdown();

		void					ConfigurationChanged();

		IDatabaseManager		DatabaseManager { get; }
		ICloudSyncManager		CloudSyncMgr { get; }
		IDataProvider			DataProvider { get; }
		ISearchProvider			SearchProvider { get; }
		ILibraryBuilder			LibraryBuilder { get; }
		IPlayQueue				PlayQueue { get; }
		IPlayHistory			PlayHistory { get; }
		IPlayListMgr			PlayListMgr { get; }
		IPlayController			PlayController { get; }
		ITagManager				TagManager { get; }
		IDataExchangeManager	DataExchangeMgr { get; }
	}
}
