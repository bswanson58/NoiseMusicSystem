using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class TagProvider : BaseProvider<DbTag>, ITagProvider {
		public TagProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddTag( DbTag tag ) {
			AddItem( tag );
		}

	    public void DeleteTag( DbTag tag ) {
	        RemoveItem( tag );
	    }

	    public DbTag GetTag( long dbid ) {
            return GetItemByKey( dbid );
        }

        public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			var context = CreateContext();

			return( new EfProviderList<DbTag>( context, Set( context ).Where( entry => entry.TagGroup == forGroup )));
		}

        public IDataUpdateShell<DbTag> GetTagForUpdate( long dbid ) {
            return( GetUpdateShell( dbid ));
        }
	}
}
