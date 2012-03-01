using System.Data.Entity;
using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class RootFolderProvider : BaseProvider<RootFolder>, IRootFolderProvider {
		public RootFolderProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddRootFolder( RootFolder folder ) {
			AddItem( folder );
		}

		public RootFolder GetRootFolder( long folderId ) {
			RootFolder	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).Include( entity => entity.FolderStrategy ).FirstOrDefault( entity => entity.DbId == folderId );
			}

			return( retValue );
		}

		public IDataProviderList<RootFolder> GetRootFolderList() {
			var	context = CreateContext();

			return( new EfProviderList<RootFolder>( context, Set( context ).Include( entity => entity.FolderStrategy )));
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			var	context = CreateContext();

			return( new EfUpdateShell<RootFolder>( context, Set( context ).Include( entity => entity.FolderStrategy )
																		  .FirstOrDefault( entity => entity.DbId == folderId )));
		}
	}
}
