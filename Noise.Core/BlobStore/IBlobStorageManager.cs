using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BlobStore {
	public interface IBlobStorageManager {
		bool			OpenStorage( string storageName );
		bool			CreateStorage( string storageName );
		void			CloseStorage();

		IBlobStorage	GetStorage();
	}

	public interface IBlobStorageResolver {
		uint		StorageLevels { get; }
		string		KeyForStorageLevel( long blobId, uint level );
	}
}
