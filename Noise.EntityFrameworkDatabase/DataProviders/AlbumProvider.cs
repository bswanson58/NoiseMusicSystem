using System.Collections.Generic;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class AlbumProvider : BaseProvider<DbAlbum>, IAlbumProvider {
		public AlbumProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddAlbum( DbAlbum album ) {
			AddItem( album );
		}

		public void DeleteAlbum( DbAlbum album ) {
			RemoveItem( album );
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( GetItemByKey( dbid ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			Condition.Requires( track ).IsNotNull();

			return( GetItemByKey( track.Album ));
		}

		public IDataProviderList<DbAlbum> GetAllAlbums() {
			return( GetListShell());
		}

		public IDataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			Condition.Requires( forArtist ).IsNotNull();

			return( GetAlbumList( forArtist.DbId ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			var context = CreateContext();

			return( new EfProviderList<DbAlbum>( context, Set( context ).Where( entity => entity.Artist == artistId )));
		}

		public IDataProviderList<DbAlbum> GetFavoriteAlbums() {
			var context = CreateContext();

			return( new EfProviderList<DbAlbum>( context, Set( context ).Where( entity => entity.IsFavorite )));
		}

		public IDataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( GetUpdateShell( albumId ));
		}

		public AlbumSupportInfo GetAlbumSupportInfo( long albumId ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<long> GetAlbumsInCategory( long categoryId ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<long> GetAlbumCategories( long albumId ) {
			throw new System.NotImplementedException();
		}

		public void SetAlbumCategories( long artistId, long albumId, IEnumerable<long> categories ) {
			throw new System.NotImplementedException();
		}

		public long GetItemCount() {
			return( GetEntityCount());
		}
	}
}
