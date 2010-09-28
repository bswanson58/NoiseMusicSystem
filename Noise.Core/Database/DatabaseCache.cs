using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Core.Database {
	internal class DatabaseCache<T> where T : class {
		private readonly List<T>	mCacheList;

		public DatabaseCache( IEnumerable<T> cacheSource ) {
			mCacheList = new List<T>( cacheSource );
		}

		public List<T> List {
			get{ return( mCacheList ); }
		}

		public void Add( T item ) {
			mCacheList.Add( item );
		}

		public T Find( Func<T, bool> findConstraint ) {
			return( mCacheList.SingleOrDefault( findConstraint ));
		}

		public List<T> FindList( Func<T, bool> findConstraint ) {
			return( mCacheList.Where( findConstraint ).ToList());
		}

		public void Clear() {
			mCacheList.Clear();
		}
	}
}
