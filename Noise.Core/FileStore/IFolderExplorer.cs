using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Core.FileStore {
	public interface IFolderExplorer {
		IEnumerable<RootFolder>	RootFolderList();

		void	SynchronizeDatabaseFolders();
		void	Stop();
	}
}
