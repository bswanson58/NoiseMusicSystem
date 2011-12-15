using System;
using System.Collections.Generic;
using System.Diagnostics;
using MbUnit.Framework;
using Noise.Core.Database;

namespace Noise.Core.Tests.Database {
	internal class CacheObject {
		public int Id { get; private set; }

		public CacheObject( int id ) {
			Id = id;
		}
	}

	public class DatabaseCacheTest {
		private static List<CacheObject>	mCacheList = new List<CacheObject> { new CacheObject( 1 ), new CacheObject( 2 ), new CacheObject( 3 ) };
		
		[Test]
		[ExpectedException( typeof( Exception ))]
		public void PreventNullSource() {
			new DatabaseCache<CacheObject>( null );
		}

		[Test]
		public void DoesRetainList() {
			var cache = new DatabaseCache<CacheObject>( mCacheList );

			Assert.AreEqual( 3, cache.List.Count );
		}

		[Test]
		public void CanConstructWithEmptyList() {
			var cacheList = new List<CacheObject>();
			
			new DatabaseCache<CacheObject>( cacheList );
		}

		[Test]
		public void CanConstructEmptyCache() {
			var cacheList = new List<CacheObject>();
			var cache = new DatabaseCache<CacheObject>( cacheList );

			Assert.IsNotNull( cache.List );
			Assert.AreEqual( 0, cache.List.Count );
		}

		[Test]
		public void CanConstructWithList() {
			var cache = new DatabaseCache<CacheObject>( mCacheList );

			Assert.IsNotNull( cache.List );
			Assert.AreEqual( 3, cache.List.Count );
		}

		[Test]
		public void CanAddCacheItems() {
			var cacheList = new List<CacheObject>();
			var cache = new DatabaseCache<CacheObject>( cacheList );

			cache.Add( new CacheObject( 1 ));

			Assert.AreEqual( 1, cache.List.Count );
		}

		[Test]
		public void CanFindCacheItem() {
			var cache = new DatabaseCache<CacheObject>( mCacheList );

			var located = cache.Find( item => item.Id == 1 );

			Assert.IsNotNull( located );
			Assert.AreEqual( 1, located.Id );
		}

		[Test]
		public void CanFindCacheList() {
			var cache = new DatabaseCache<CacheObject>( mCacheList );

			var located = cache.FindList( item => item.Id < 3 );

			Assert.IsNotNull( located );
			Assert.AreEqual( 2, located.Count );
		}

		[Test]
		public void CanClearEmptyCacheList() {
			var cache = new DatabaseCache<CacheObject>( new List<CacheObject>());

			cache.Clear();

			Assert.IsNotNull( cache.List );
			Assert.AreEqual( 0, cache.List.Count );
		}
	}
}
