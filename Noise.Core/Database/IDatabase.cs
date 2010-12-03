using Noise.Infrastructure.Dto;

namespace Noise.Core.Database {
	public interface IDatabase {
		string				DatabaseId { get; }
		Eloquera.Client.DB	Database { get; }
		DbVersion			DatabaseVersion { get; }

		bool		InitializeDatabase();
		bool		OpenWithCreateDatabase();
		bool		OpenDatabase();
		bool		InitializeAndOpenDatabase();

		DbBase		ValidateOnThread( DbBase dbObject );

		void		Insert( object dbObject );
		void		Store( object dbObject );
		void		Delete( object dbObject );

		void		CloseDatabase();
	}
}
