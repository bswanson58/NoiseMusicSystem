using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Noise.RavenDatabase.Support;

namespace Noise.RavenDatabase.DataProviders {
	public class TagAssociationProvider : BaseProvider<DbTagAssociation>, ITagAssociationProvider {
		public TagAssociationProvider( IDbFactory databaseFactory ) :
			base( databaseFactory, entity => new object[] { entity.DbId } ) {
		}

		public DbTagAssociation GetAlbumTagAssociation( long albumId, long tagId ) {
			return( Database.Get( entity => entity.AlbumId == albumId && entity.TagId == tagId ));
		}

		public IDataProviderList<DbTagAssociation> GetArtistTagList( long artistId, eTagGroup tagGroup ) {
			return( Database.Find( entity => entity.ArtistId == artistId && entity.TagGroup == tagGroup ));
		}

		public IDataProviderList<DbTagAssociation> GetAlbumTagList( long albumId, eTagGroup tagGroup ) {
			return( Database.Find( entity => entity.AlbumId == albumId && entity.TagGroup == tagGroup ));
		}

		public IDataProviderList<DbTagAssociation> GetTagList( eTagGroup tagGroup, long tagId ) {
			return(  Database.Find( entity => entity.TagGroup == tagGroup && entity.TagId == tagId ));
		}

		public void AddAssociation( DbTagAssociation item ) {
			Database.Add( item );
		}

		public void RemoveAssociation( long tagId ) {
			Database.Delete( entity => entity.DbId == tagId );
		}
	}
}
