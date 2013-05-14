using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TagProvider : ITagProvider {
		private readonly IDbFactory			mDbFactory;
		private readonly IRepository<DbTag>	mDatabase;

		public TagProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbTag>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public void AddTag( DbTag tag ) {
			mDatabase.Add( tag );
		}

		public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			return( new RavenDataProviderList<DbTag>( mDatabase.Find( tag => tag.TagGroup == forGroup )));
		}
	}
}
