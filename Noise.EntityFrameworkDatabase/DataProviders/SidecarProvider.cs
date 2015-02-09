using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	internal class SidecarProvider : BaseProvider<StorageSidecar>, ISidecarProvider {
		public SidecarProvider( IContextProvider contextProvider ) :
			base( contextProvider ) {
		}

		public void Add( StorageSidecar sidecar ) {
			AddItem( sidecar );
		}

		public void Delete( StorageSidecar sidecar ) {
			RemoveItem( sidecar );
		}

		public StorageSidecar GetSidecar( long dbid ) {
			return( GetItemByKey( dbid ));
		}

		public StorageSidecar GetSidecarForArtist( DbArtist artist ) {
			Condition.Requires( artist ).IsNotNull();

			StorageSidecar	retValue;

			using( var context = CreateContext() ) {
				retValue = Set( context ).FirstOrDefault( s => s.ArtistId == artist.DbId && s.AlbumId == Constants.cDatabaseNullOid );
			}

			return( retValue );
		}

		public StorageSidecar GetSidecarForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			StorageSidecar	retValue;

			using( var context = CreateContext() ) {
				retValue = Set( context ).FirstOrDefault( s => s.AlbumId == album.DbId );
			}

			return( retValue );
		}

		public IDataProviderList<StorageSidecar> GetUnreadSidecars() {
			var context = CreateContext();

			return( new EfProviderList<StorageSidecar>( context, Set( context ).Where( sidecar => sidecar.Status == SidecarStatus.Unread )));
		}

		public IDataUpdateShell<StorageSidecar> GetSidecarForUpdate( long dbid ) {
			return( GetUpdateShell( dbid ));
		}
	}
}
