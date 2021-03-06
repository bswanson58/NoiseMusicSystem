﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CuttingEdge.Conditions;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Raven.Client;

namespace Noise.RavenDatabase.Support {
	internal class RavenRepository<T> : IRepository<T> where T : class {
		private	readonly IDocumentStore		mDatabase;
		private readonly ILogRaven			mLog;
		private readonly Func<T, object[]>	mKeySelector; 

		public RavenRepository( IDocumentStore database, Func<T, object[]> keySelector, ILogRaven log ) {
			mDatabase = database;
			mKeySelector = keySelector;
			mLog = log;

			mDatabase.Conventions.RegisterIdConvention<T>(( databaseName, commands, o ) => KeyGenerator( mKeySelector( o )));
//			mDatabase.Conventions.DocumentKeyGenerator = ( c, o ) => KeyGenerator( mKeySelector( o as T ));
		}

		private string KeyGenerator( IEnumerable<Object> keys ) {
			var strings = new List<String>();

			foreach( var x in keys ) {
				if( x is DateTime ) {
					strings.Add(((DateTime)x ).Ticks.ToString( CultureInfo.InvariantCulture ));
				}
				else {
					strings.Add( x.ToString());
				}
			}

			// Replace any illegal characters with hyphens
			strings = strings.Select( x => x.Replace( '/', '-' ).Replace( '\\', '-' ).Replace( ' ', '-' ).Replace( '^', '-' )).ToList();

			return( mDatabase.Conventions.GetTypeTagName( typeof( T )) + "/" + strings.Aggregate(( x, y ) => x + "/" + y ));
		}

		public bool Exists( T item ) {
			Condition.Requires( item ).IsNotNull();

			return( Exists( mKeySelector( item )));
		}

		public bool Exists( object key ) {
			return( Exists( new [] { key } ));
		}

		public bool Exists( object[] keys ) {
			bool retValue;

			using( var session = mDatabase.OpenSession()) {
				retValue = session.Load<T>( KeyGenerator( keys )) != null;
			}

			return( retValue );
		}

		public bool Exists( Expression<Func<T, bool>> expression ) {
			bool retValue;

			using( var session = mDatabase.OpenSession()) {
				retValue = session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).FirstOrDefault( expression ) != null;
			}

			return( retValue );
		}

		public T Get( object key ) {
			return( Get( new [] { key }));
		}

		public T Get( object[] keys ) {
			T	retValue;

			using( var session = mDatabase.OpenSession()) {
				retValue = session.Load<T>( KeyGenerator( keys ));
			}

			return( retValue );
		}

		public void Delete( T item ) {
			Condition.Requires( item ).IsNotNull();

			using( var session = mDatabase.OpenSession()) {
				var repositoryItem = session.Load<T>( KeyGenerator( mKeySelector( item )));

				if( repositoryItem != null ) {
					session.Delete( repositoryItem );

					session.SaveChanges();

					mLog.RemoveItem( repositoryItem );
				}
				else {
					mLog.RemoveUnknownItem( item );
				}
			}
		}

		public void Delete( Expression<Func<T, bool>> expression ) {
			using( var session = mDatabase.OpenSession()) {
				var query = session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).Where( expression );

				foreach( var entity in query ) {
					session.Delete( entity );

					mLog.RemoveItem( entity );
				}

				session.SaveChanges();
			}
		}

		public void DeleteAll() {
			using( var session = mDatabase.OpenSession()) {
				var query = session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow());

				foreach( var entity in query ) {
					session.Delete( entity );

					mLog.RemoveItem( entity );
				}

				session.SaveChanges();
			}
		}

		public T Get( Expression<Func<T, bool>> expression ) {
			T	retValue;

			using( var session = mDatabase.OpenSession()) {
				retValue = session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).FirstOrDefault( expression );
			}

			return ( retValue );
		}

		public IEnumerable<T> Match( Expression<Func<T, bool>> expression ) {
			var retValue = new List<T>();

			using( var session = mDatabase.OpenSession()) {
				retValue.AddRange( session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).Where( expression ));
			}

			return( retValue );
		} 

		public IDataProviderList<T> Find( Expression<Func<T, bool>> expression ) {
			return( new QuerySession<T>( mDatabase, expression ));
		}

		public IDataProviderList<T> Find( Expression<Func<T, bool>> expression, string indexName ) {
			return ( new QuerySession<T>( mDatabase, expression, indexName ));
		}

		public IDataProviderList<T> FindAll() {
			return ( new QuerySession<T>( mDatabase ));
		}

		public void Add( T item ) {
			Condition.Requires( item ).IsNotNull();

			if(!Exists( item )) {
				using( var session = mDatabase.OpenSession() ) {
					session.Store( item );

					session.SaveChanges();

					mLog.AddingItem( item );
				}
			}
			else {
				mLog.AddingExistingItem( item );
			}
		}

		public void Add( IEnumerable<T> items ) {
			using( var session = mDatabase.OpenSession()) {
				foreach( var entity in items ) {
					if( session.Load<T>( KeyGenerator( mKeySelector( entity ))) == null ) {
						session.Store( entity );

						mLog.AddingItem( entity );
					}
				}

				session.SaveChanges();
			}
		}

		public void Update( T item ) {
			Condition.Requires( item ).IsNotNull();

			using( var session = mDatabase.OpenSession() ) {
				var repositoryItem = session.Load<T>( KeyGenerator( mKeySelector( item )));

				if( repositoryItem != null ) {
					AutoMapper.Mapper.Map( item, repositoryItem );

					session.SaveChanges();
				}
				else {
					mLog.UpdateUnknownItem( item );
				}
			}
		}

		public long Count() {
			long retValue;

			using( var session = mDatabase.OpenSession()) {
				retValue = session.Query<T>().Count();
			}

			return( retValue );
		}
	}
}
