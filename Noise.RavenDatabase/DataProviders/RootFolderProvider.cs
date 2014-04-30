using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class RootFolderProvider : BaseProvider<RootFolder>, IRootFolderProvider {
		private	long	mFirstScanCompleted;

		public RootFolderProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId }) {
		}

		public void AddRootFolder( RootFolder folder ) {
			Database.Add( folder );
		}

		public RootFolder GetRootFolder( long folderId ) {
			return( Database.Get( folderId ));
		}

		public void DeleteRootFolder( RootFolder folder ) {
			Database.Delete( folder );
		}

		public IDataProviderList<RootFolder> GetRootFolderList() {
			return( Database.FindAll());
		}

		public IDataUpdateShell<RootFolder> GetFolderForUpdate( long folderId ) {
			return( new RavenDataUpdateShell<RootFolder>( folder => Database.Update( folder ), Database.Get( folderId )));
		}

		public long FirstScanCompleted() {
			var retValue = mFirstScanCompleted;

			if( retValue == 0 ) {
				using( var folderList = GetRootFolderList() ) {
					mFirstScanCompleted = folderList.List.Max( folder => folder.InitialScanCompleted );
				}

				retValue = mFirstScanCompleted;
			}

			return( retValue );
		}
	}
}
