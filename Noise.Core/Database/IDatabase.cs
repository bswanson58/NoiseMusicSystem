namespace Noise.Core.Database {
	public interface IDatabase {
		string				DatabaseId { get; }
		Eloquera.Client.DB	Database { get; }

		bool		InitializeDatabase();
		bool		OpenWithCreateDatabase();
		bool		OpenDatabase();
		bool		InitializeAndOpenDatabase();

		object		ValidateOnThread( object dbObject );

		void		Insert( object dbObject );
		void		Store( object dbObject );
		void		Delete( object dbObject );

		void		CloseDatabase();
	}
}
