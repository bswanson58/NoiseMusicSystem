using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;

namespace Noise.RavenDatabase.Support {
	public class QuerySession<T> : IQuerySession<T> where T : class {
		private	const int							cTakeCount = 1024;

		private	readonly IDocumentStore				mDatabase;
		private readonly Expression<Func<T, bool>>	mExpression;
		private	IDocumentSession					mSession;
		private IQueryable<T>						mQuery;

		public QuerySession( IDocumentStore database ) {
			mDatabase = database;
		}

		public QuerySession( IDocumentStore database, Expression<Func<T, bool>> expression ) :
			this( database ) {
			mDatabase = database;
			mExpression = expression;
		}

		public void Dispose() {
			if( mSession != null ) {
				mSession.Dispose();
			}
		}
	 
		public IEnumerable<T> List {
			get {
				var start = 0;

				while( true ) {
					var sessionQueryCount = 0;

					using( var session = mDatabase.OpenSession()) {
						while( sessionQueryCount < 30 ) {
							var query = mExpression != null ? session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).Where( mExpression ).Take( cTakeCount ).Skip( start ).ToList() :
															  session.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).Take( cTakeCount ).Skip( start ).ToList();
							var queryCount = query.Count;

							if( queryCount == 0 ) {
								yield break;
							}

							foreach( T item in query ) {
								yield return item;
							}

							start += queryCount;
							sessionQueryCount++;
						}
					}
				}
			}
		}

		public IQueryable<T> Query() {
			if( mSession == null ) {
				mSession = mDatabase.OpenSession();
			}

			if( mQuery == null ) {
				mQuery = mExpression != null ? mSession.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow()).Where( mExpression ) :
											   mSession.Query<T>().Customize( x => x.WaitForNonStaleResultsAsOfNow());
			}

			return ( mQuery );
		}
	}
}
