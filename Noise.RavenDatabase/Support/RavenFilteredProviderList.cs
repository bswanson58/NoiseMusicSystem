using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class RavenFilteredProviderList<T> : IDataProviderList<T> where T : DbArtist {
		private readonly IDatabaseFilter	mFilter;
		private IQuerySession<T>			mQuerySession;
		private readonly IList<T>			mList; 

		public IEnumerable<T> List { get; private set; }

		public RavenFilteredProviderList( IQuerySession<T> querySession, IDatabaseFilter filter ) {
			mFilter = filter;
			mQuerySession = querySession;
			
			mList = mQuerySession.Query().ToList();
			List = from entity in mList where mFilter.ArtistMatch( entity ) select entity;
		}
			public void Dispose() {
			if( mQuerySession != null ) {
				mQuerySession.Dispose();

				mQuerySession = null;
			}
		}
	}
}
