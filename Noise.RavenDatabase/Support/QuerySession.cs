using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Noise.Infrastructure.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace Noise.RavenDatabase.Support {
	public class QuerySession<T> : IDataProviderList<T> where T : class {
		private	const int							cTakeCount = 1024;

		private	readonly IDocumentStore				mDatabase;
		private readonly Expression<Func<T, bool>>	mExpression;
		private readonly string						mIndexName;

		public QuerySession( IDocumentStore database ) {
			mDatabase = database;

			mIndexName = string.Empty;
		}

		public QuerySession( IDocumentStore database, string indexName ) :
			this( database ) {
			mIndexName = indexName;
		}

		public QuerySession( IDocumentStore database, Expression<Func<T, bool>> expression ) :
			this( database ) {
			mExpression = expression;
		}

		public QuerySession( IDocumentStore database, Expression<Func<T, bool>> expression, string indexName ) :
			this( database, expression ) {
			mIndexName = indexName;
		}

		public void Dispose() {
		}
	 
		public IEnumerable<T> List {
			get {
				var start = 0;

				while( true ) {
					var sessionQueryCount = 0;

					using( var session = mDatabase.OpenSession()) {
						while( sessionQueryCount < 30 ) {
							var query = string.IsNullOrWhiteSpace( mIndexName ) ? session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()) :
																				  session.Query<T>( mIndexName ).Customize( x => x.WaitForNonStaleResultsAsOfNow());
							if( mExpression != null ) {
								query = query.Where( mExpression );
							}

							var queryList = query.Take( cTakeCount ).Skip( start ).ToList();
							var queryCount = queryList.Count;

							if( queryCount == 0 ) {
								yield break;
							}

							foreach( T item in queryList ) {
								yield return item;
							}

							start += queryCount;
							sessionQueryCount++;
						}
					}
				}
			}
		}
	}
}
