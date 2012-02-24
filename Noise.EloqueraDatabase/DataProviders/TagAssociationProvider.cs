using System.Collections.Generic;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.DataProviders {
	internal class TagAssociationProvider : BaseDataProvider<DbTagAssociation>, ITagAssociationProvider {
		public TagAssociationProvider( IEloqueraManager databaseManager ) :
			base( databaseManager ) { }

		public IDataProviderList<DbTagAssociation> GetArtistTagList( long artistId, eTagGroup tagGroup ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND ArtistId = @artistId", 
												new Dictionary<string, object> {{ "artistId", artistId }, { "group", eTagGroup.User }}, "Exception - GetArtistTagList" ));
		}

		public IDataProviderList<DbTagAssociation> GetAlbumTagList( long albumId, eTagGroup tagGroup ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND AlbumId = @albumId", 
												new Dictionary<string, object> {{ "albumId", albumId }, { "group", eTagGroup.User }}, "Exception - GetAlbumTagList" ));
		}

		public IDataProviderList<DbTagAssociation> GetTagList( eTagGroup tagGroup, long tagId ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND TagId = @tagId", new Dictionary<string, object> {{ "group", tagGroup }, { "tagId", tagId }}, "Exception - GetTagList" ));
		}

		public DbTagAssociation GetAlbumTagAssociation( long albumId, long tagId ) {
			return( TryGetItem( "SELECT DbTagAssociation Where AlbumId = @albumId AND TagId = @tagId",
												new Dictionary<string, object> {{ "albumId", albumId }, { "tagId", tagId }}, "GetAlbumTagAssociation" ));
		}

		public void AddAssociation( DbTagAssociation item ) {
			InsertItem( item );
		}

		private DbTagAssociation GetAssociation( long itemId ) {
			return( TryGetItem( "SELECT DbTagAssociation Where DbId = @itemId", new Dictionary<string, object> {{ "itemId", itemId }}, "Exception - GetAssociation" ));
		}

		public void RemoveAssociation( long itemId ) {
			var item = GetAssociation( itemId );

			if( item != null ) {
				DeleteItem( item );
			}
		}
	}
}
