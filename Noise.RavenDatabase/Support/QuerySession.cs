using System;
using System.Linq;
using System.Linq.Expressions;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;

namespace Noise.RavenDatabase.Support {
	public class QuerySession<T> : IQuerySession<T> where T : class {
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

		public IQueryable<T> Query() {
			if( mSession == null ) {
				mSession = mDatabase.OpenSession();
			}

			if( mQuery == null ) {
				mQuery = mExpression != null ? mSession.Query<T>().Where( mExpression ) :
											   mSession.Query<T>();
			}

			return ( mQuery );
		}
	}
}
