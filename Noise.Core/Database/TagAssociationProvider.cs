using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Database {
	internal class TagAssociationProvider : BaseDataProvider<DbTagAssociation>, ITagAssociationProvider {
		public TagAssociationProvider( IDatabaseManager databaseManager ) :
			base( databaseManager ) { }

		public DataProviderList<DbTagAssociation> GetArtistTagList( long artistId, eTagGroup tagGroup ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND ArtistId = @artistId", 
												new Dictionary<string, object> {{ "artistId", artistId }, { "group", eTagGroup.User }}, "Exception - GetArtistTagList" ));
		}

		public DataProviderList<DbTagAssociation> GetAlbumTagList( long albumId, eTagGroup tagGroup ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND AlbumId = @albumId", 
												new Dictionary<string, object> {{ "albumId", albumId }, { "group", eTagGroup.User }}, "Exception - GetAlbumTagList" ));
		}

		public DataProviderList<DbTagAssociation> GetTagList( eTagGroup tagGroup, long tagId ) {
			return( TryGetList( "SELECT DbTagAssociation Where TagGroup = @group AND TagId = @tagId", new Dictionary<string, object> {{ "group", tagGroup }, { "tagId", tagId }}, "Exception - GetTagList" ));
		}

		public void AddAssociation( DbTagAssociation item ) {
			InsertItem( item );
		}

		private DbTagAssociation GetAssociation( long tagId ) {
			return( TryGetItem( "SELECT DbTagAssociation Where DbId = @itemId", new Dictionary<string, object> {{ "tagId", tagId }}, "Exception - GetAssociation" ));
		}

		public void RemoveAssociation( long tagId ) {
			var item = GetAssociation( tagId );

			if( item != null ) {
				DeleteItem( item );
			}
		}
	}
}
