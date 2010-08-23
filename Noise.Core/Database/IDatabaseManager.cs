namespace Noise.Core.Database {
	public interface IDatabaseManager {
		Eloquera.Client.DB	Database { get; }

		bool		InitializeDatabase();
		bool		OpenWithCreateDatabase();
		bool		OpenDatabase();

		void		CloseDatabase();
	}
}
