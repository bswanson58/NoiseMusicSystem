using System.Collections;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabase {
		string			DatabaseId { get; }
		DbVersion		DatabaseVersion { get; }
		IBlobStorage	BlobStorage { get; set; }
		bool			IsOpen { get; }

		bool			InitializeDatabase();
		bool			OpenWithCreateDatabase();
		bool			InitializeAndOpenDatabase();

//		DbBase		ValidateOnThread( DbBase dbObject );

		object			QueryForItem( string query );
		object			QueryForItem( string query, IDictionary<string, object> parameters );

		IEnumerable		QueryForList( string query );
		IEnumerable		QueryForList( string query, IDictionary<string, object> parameters );

		void			InsertItem( object item );
		void			UpdateItem( object item );
		void			DeleteItem( object item );

		void		CloseDatabase();
	}
}
