using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class DiscographyProvider : BaseProvider<DbDiscographyRelease>, IDiscographyProvider {
		public DiscographyProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddDiscography( DbDiscographyRelease release ) {
			AddItem( release );
		}

		public void RemoveDiscography( DbDiscographyRelease release ) {
			RemoveItem( release );
		}

		public IDataProviderList<DbDiscographyRelease> GetDiscography( long artistId ) {
			var context = CreateContext();

			return( new EfProviderList<DbDiscographyRelease>( context, from entry in Set( context ) where entry.Artist == artistId select entry ));
		}
	}
}
