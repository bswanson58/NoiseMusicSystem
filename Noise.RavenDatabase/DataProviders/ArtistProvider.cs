using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class ArtistProvider : IArtistProvider {
		private readonly IDbFactory				mDbFactory;
		private readonly IRepository<DbArtist>	mDatabase;

		public ArtistProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepositoryT<DbArtist>( mDbFactory.GetLibraryDatabase(), artist => new object[] { artist.DbId });
		}
		public void AddArtist( DbArtist artist ) {
			mDatabase.Add( artist );
		}

		public void DeleteArtist( DbArtist artist ) {
			throw new System.NotImplementedException();
		}

		public DbArtist GetArtist( long dbid ) {
			throw new System.NotImplementedException();
		}

		public DbArtist GetArtistForAlbum( DbAlbum album ) {
			throw new System.NotImplementedException();
		}

		public DbArtist FindArtist( string artistName ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetArtistList() {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetArtistList( IDatabaseFilter filter ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetChangedArtists( long changedSince ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<DbArtist> GetFavoriteArtists() {
			throw new System.NotImplementedException();
		}

		public IDataUpdateShell<DbArtist> GetArtistForUpdate( long artistId ) {
			throw new System.NotImplementedException();
		}

		public void UpdateArtistLastChanged( long artistId ) {
			throw new System.NotImplementedException();
		}

		public IDataProviderList<long> GetArtistCategories( long artistId ) {
			throw new System.NotImplementedException();
		}

		public long GetItemCount() {
			throw new System.NotImplementedException();
		}
	}
}
