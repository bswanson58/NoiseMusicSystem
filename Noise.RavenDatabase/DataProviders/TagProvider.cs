using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Logging;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	internal class TagProvider : BaseProvider<DbTag>, ITagProvider {
		public TagProvider( IDbFactory databaseFactory, ILogRaven log ) :
			base( databaseFactory, entity => new object[] { entity.DbId }, log ) {
		}

		public void AddTag( DbTag tag ) {
			Database.Add( tag );
		}

        public IDataUpdateShell<DbTag> GetTagForUpdate( long dbid ) {
            return( new RavenDataUpdateShell<DbTag>( entity => Database.Update( entity ), Database.Get( dbid )));
        }

        public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			return( Database.Find( tag => tag.TagGroup == forGroup ));
		}
	}
}
