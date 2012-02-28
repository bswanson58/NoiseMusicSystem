using System.Linq;
using CuttingEdge.Conditions;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class ArtistProvider : BaseProvider<DbArtist>, IArtistProvider {
		public ArtistProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public void AddArtist( DbArtist artist ) {
			AddItem( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			RemoveItem( artist );
		}

		public DbArtist GetArtist( long dbid ) {
			return( GetItemByKey( dbid ));
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			Condition.Requires( album ).IsNotNull();

			return( GetItemByKey( album.Artist ));
		}

		public IDataProviderList<DbArtist> GetArtistList() {
			return( GetListShell());
		}

		public IDataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			var context = CreateContext();

			return( new EfProviderList<DbArtist>( context, Set( context ).Where( entity => entity.IsFavorite )));
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			return( GetUpdateShell( artistId ));
		}

		public void UpdateArtistLastChanged( long artistId ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<long> GetArtistCategories( long artistId ) {
			throw new System.NotImplementedException();
		}

		public ArtistSupportInfo GetArtistSupportInfo( long artistId ) {
			throw new System.NotImplementedException();
		}

		public long GetItemCount() {
			return( GetEntityCount());
		}
	}
}
