namespace Noise.Core.FileStore {
	public interface IFolderExplorer {
		void	SynchronizeDatabaseFolders();
		void	Stop();
	}
}
