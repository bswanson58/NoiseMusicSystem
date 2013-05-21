using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Noise.Infrastructure.Interfaces;

namespace Noise.RavenDatabase.Interfaces {
	public interface IRepository<T> where T : class {
		void					Add( T item );
		void					Add( IEnumerable<T> items );

		void					Update( T item );

		void					Delete( T item );
		void					Delete( Expression<Func<T, bool>> expression );
		void					DeleteAll();

		bool					Exists( T item );
		bool					Exists( object key );
		bool					Exists( object[] keys );
		bool					Exists( Expression<Func<T, bool>> expression );

		IDataProviderList<T>	FindAll();
		IDataProviderList<T>	Find( Expression<Func<T, bool>> expression );
		IDataProviderList<T>	Find( Expression<Func<T, bool>> expression, string indexName );

		T						Get( Expression<Func<T, bool>> expression );
		T						Get( object key );
		T						Get( object[] keys );

		long					Count();
	}
}
