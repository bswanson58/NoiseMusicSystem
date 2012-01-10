using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.BlobStore {
	public interface IBlobStorageManager {
		bool			OpenStorage( string storageName );
		bool			CreateStorage( string storageName );
		void			CloseStorage();
		void			DeleteStorage( string storageName );

		bool			IsOpen { get; }
		IBlobStorage	GetStorage();
	}
}
