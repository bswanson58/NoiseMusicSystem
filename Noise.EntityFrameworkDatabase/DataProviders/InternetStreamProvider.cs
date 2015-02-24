using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.EntityFrameworkDatabase.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class InternetStreamProvider : BaseProvider<DbInternetStream>, IInternetStreamProvider {
		public InternetStreamProvider( IContextProvider contextProvider, ILogDatabase log ) :
			base( contextProvider, log ) { }

		public void AddStream( DbInternetStream stream ) {
			AddItem( stream );
		}

		public void DeleteStream( DbInternetStream stream ) {
			RemoveItem( stream );
		}

		public DbInternetStream GetStream( long streamId ) {
			return( GetItemByKey( streamId ));
		}

		public IDataProviderList<DbInternetStream> GetStreamList() {
			return( GetListShell());
		}

		public IDataUpdateShell<DbInternetStream> GetStreamForUpdate( long streamId ) {
			return( GetUpdateShell( streamId ));
		}
	}
}
