using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IStorageFolderProvider {
		StorageFolder	GetFolder( long folderId );
	}
}
