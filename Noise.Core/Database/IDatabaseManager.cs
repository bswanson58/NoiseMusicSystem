namespace Noise.Core.Database {
	public interface IDatabaseManager {
		Eloquera.Client.DB	Database { get; }

		bool		InitializeDatabase();
		void		OpenWithCreateDatabase();
		bool		OpenDatabase();

		void		CloseDatabase();
	}
}
