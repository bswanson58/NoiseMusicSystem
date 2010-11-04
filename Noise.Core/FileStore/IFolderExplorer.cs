using System.Collections.Generic;

namespace Noise.Core.FileStore {
	public interface IFolderExplorer {
		IEnumerable<RootFolder>	RootFolderList();

		void	SynchronizeDatabaseFolders();
		void	Stop();
	}
}
