using System;
using System.Collections;
using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseShell : IDisposable {
		IDatabase	Database { get; }

		object		QueryForItem( string query );
		object		QueryForItem( string query, IDictionary<string, object> parameters );

		IEnumerable	QueryForList( string query );
		IEnumerable	QueryForList( string query, IDictionary<string, object> parameters );

		void		InsertItem( object item );
		void		UpdateItem( object item );
		void		DeleteItem( object item );

		void		FreeDatabase();
	}
}
