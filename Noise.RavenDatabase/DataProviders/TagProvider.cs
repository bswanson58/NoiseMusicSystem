using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TagProvider : BaseProvider<DbTag>, ITagProvider {
		public TagProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddTag( DbTag tag ) {
			Database.Add( tag );
		}

		public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			return( new RavenDataProviderList<DbTag>( Database.Find( tag => tag.TagGroup == forGroup )));
		}
	}
}
