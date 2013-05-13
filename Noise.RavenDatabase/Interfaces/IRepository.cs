using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Noise.RavenDatabase.Interfaces {
	public interface IRepository<T> where T : class {
		void				Add( T item );
		void				Add( IEnumerable<T> items );

		void				Update( T item );

		void				Delete( T item );
		void				Delete( Expression<Func<T, bool>> expression );
		void				DeleteAll();

		bool				Exists( object key );
		bool				Exists( object[] keys );
		bool				Exists( Expression<Func<T, bool>> expression );

		IQuerySession<T>	FindAll();
		IQuerySession<T>	Find( Expression<Func<T, bool>> expression );

		T					Get( Expression<Func<T, bool>> expression );
		T					Get( object key );
		T					Get( object[] keys );

		long				Count();
	}
}
