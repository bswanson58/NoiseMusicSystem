namespace Noise.Infrastructure.Interfaces {
	public interface INoiseManager {
		bool					Initialize();
		void					StartExplorerJobs();
		void					Shutdown();

		IDatabaseManager		DatabaseManager { get; }
		IDataProvider			DataProvider { get; }
		ISearchProvider			SearchProvider { get; }
		ITagManager				TagManager { get; }
	}
}
