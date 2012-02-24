using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Interfaces {
	public interface IDatabaseFactory {
		IDatabase		GetDatabaseInstance();
		void			SetBlobStorageInstance( IDatabase database );

		void			CloseFactory();
	}
}
