using System.Collections;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Interfaces {
	public interface IDatabase {
		string			DatabaseId { get; }
		DbVersion		DatabaseVersion { get; }
		IBlobStorage	BlobStorage { get; set; }
		bool			IsOpen { get; }

		bool			InitializeDatabase();
		bool			OpenWithCreateDatabase();
		bool			InitializeAndOpenDatabase();

		object			QueryForItem( string query );
		object			QueryForItem( string query, IDictionary<string, object> parameters );

		IEnumerable		QueryForList( string query );
		IEnumerable		QueryForList( string query, IDictionary<string, object> parameters );

		void			InsertItem( object item );
		void			UpdateItem( object item );
		void			DeleteItem( object item );

		void			CloseDatabase();
		void			DeleteDatabase();
	}
}
