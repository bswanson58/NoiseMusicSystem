namespace Noise.Core.Database {
	public interface IDatabase {
		string				DatabaseId { get; }
		Eloquera.Client.DB	Database { get; }

		bool		InitializeDatabase();
		bool		OpenWithCreateDatabase();
		bool		OpenDatabase();
		bool		InitializeAndOpenDatabase( string clientName );

		void		CloseDatabase();
		void		CloseDatabase( string clientName );
	}
}
