using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TagProvider : BaseProvider<DbTag>, ITagProvider {
		public TagProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddTag( DbTag tag ) {
			AddItem( tag );
		}

		public IDataProviderList<DbTag> GetTagList( eTagGroup forGroup ) {
			var context = CreateContext();

			return( new EfProviderList<DbTag>( context, Set( context ).Where( entry => entry.DbTagGroup == (int)forGroup )));
		}
	}
}
