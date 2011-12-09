namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseFactory {
		IDatabase		GetDatabaseInstance();
		void			SetBlobStorageInstance( IDatabase database );

		void			CloseFactory();
	}
}
