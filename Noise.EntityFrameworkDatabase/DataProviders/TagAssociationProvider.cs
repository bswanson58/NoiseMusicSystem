using System.Linq;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DataProviders {
	public class TagAssociationProvider : BaseProvider<DbTagAssociation>, ITagAssociationProvider {
		public TagAssociationProvider( IContextProvider contextProvider ) :
			base( contextProvider ) { }

		public DbTagAssociation GetAlbumTagAssociation( long albumId, long tagId ) {
			DbTagAssociation	retValue;

			using( var context = CreateContext()) {
				retValue = Set( context ).FirstOrDefault( entity => (( entity.AlbumId == albumId ) && ( entity.TagId == tagId )));
			}
			return( retValue );
		}

		public IDataProviderList<DbTagAssociation> GetArtistTagList( long artistId, eTagGroup tagGroup ) {
			var context = CreateContext();

			return( new EfProviderList<DbTagAssociation>( context, Set( context ).Where( entity => (( entity.ArtistId == artistId ) &&
																									( entity.TagGroup == tagGroup )))));
		}

		public IDataProviderList<DbTagAssociation> GetAlbumTagList( long albumId, eTagGroup tagGroup ) {
			var context = CreateContext();

			return( new EfProviderList<DbTagAssociation>( context, Set( context ).Where( entity => (( entity.AlbumId == albumId ) &&
																									( entity.TagGroup == tagGroup )))));
		}

		public IDataProviderList<DbTagAssociation> GetTagList( eTagGroup tagGroup, long tagId ) {
			var context = CreateContext();

			return( new EfProviderList<DbTagAssociation>( context, Set( context ).Where( entity => (( entity.TagId == tagId ) &&
																									( entity.TagGroup == tagGroup )))));
		}

		public void AddAssociation( DbTagAssociation item ) {
			AddItem( item );
		}

		public void RemoveAssociation( long tagId ) {
			RemoveItem( tagId );
		}
	}
}
