using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TagAssociationProvider : ITagAssociationProvider {
		private readonly IDbFactory						mDbFactory;
		private readonly IRepository<DbTagAssociation>	mDatabase;

		public TagAssociationProvider( IDbFactory databaseFactory ) {
			mDbFactory = databaseFactory;

			mDatabase = new RavenRepository<DbTagAssociation>( mDbFactory.GetLibraryDatabase(), entity => new object[] { entity.DbId });
		}

		public DbTagAssociation GetAlbumTagAssociation( long albumId, long tagId ) {
			return( mDatabase.Get( entity => entity.AlbumId == albumId && entity.TagId == tagId ));
		}

		public IDataProviderList<DbTagAssociation> GetArtistTagList( long artistId, eTagGroup tagGroup ) {
			return( new RavenDataProviderList<DbTagAssociation>( mDatabase.Find( entity => entity.ArtistId == artistId && entity.TagGroup == tagGroup )));
		}

		public IDataProviderList<DbTagAssociation> GetAlbumTagList( long albumId, eTagGroup tagGroup ) {
			return( new RavenDataProviderList<DbTagAssociation>( mDatabase.Find( entity => entity.AlbumId == albumId && entity.TagGroup == tagGroup ) ) );
		}

		public IDataProviderList<DbTagAssociation> GetTagList( eTagGroup tagGroup, long tagId ) {
			return( new RavenDataProviderList<DbTagAssociation>( mDatabase.Find( entity => entity.TagGroup == tagGroup && entity.TagId == tagId )));
		}

		public void AddAssociation( DbTagAssociation item ) {
			mDatabase.Add( item );
		}

		public void RemoveAssociation( long tagId ) {
			mDatabase.Delete( entity => entity.DbId == tagId );
		}
	}
}
