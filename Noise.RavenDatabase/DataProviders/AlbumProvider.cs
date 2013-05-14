using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class AlbumProvider : IAlbumProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbAlbum>	mDatabase;

		public AlbumProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbAlbum>( mDbFactory.GetLibraryDatabase(), album => new object[] { album.DbId });
		}

		public void AddAlbum( DbAlbum album ) {
			mDatabase.Add( album );
		}

		public void DeleteAlbum( DbAlbum album ) {
			mDatabase.Delete( album );
		}

		public DbAlbum GetAlbum( long dbid ) {
			return( mDatabase.Get( dbid ));
		}

		public DbAlbum GetAlbumForTrack( DbTrack track ) {
			return( mDatabase.Get( track.Album ));
		}

		public IDataProviderList<DbAlbum> GetAllAlbums() {
			return( new RavenDataProviderList<DbAlbum>( mDatabase.FindAll()));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( DbArtist forArtist ) {
			return( GetAlbumList( forArtist.DbId ));
		}

		public IDataProviderList<DbAlbum> GetAlbumList( long artistId ) {
			return( new RavenDataProviderList<DbAlbum>( mDatabase.Find( album => album.Artist == artistId )));
		}

		public IDataProviderList<DbAlbum> GetFavoriteAlbums() {
			return( new RavenDataProviderList<DbAlbum>( mDatabase.Find( album => album.IsFavorite )));
		}

		public IDataUpdateShell<DbAlbum> GetAlbumForUpdate( long albumId ) {
			return( new RavenDataUpdateShell<DbAlbum>( album => mDatabase.Update( album ), mDatabase.Get( albumId )));
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
			return( mDatabase.Count());
		}
	}
}
