using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileStore {
	public interface IFolderExplorer {
		IEnumerable<RootFolder>	RootFolderList();

		void	LoadConfiguration( IDatabase database );
		void	SynchronizeDatabaseFolders();
		void	Stop();
	}
}
