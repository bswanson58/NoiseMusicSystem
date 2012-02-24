using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagAssociationProvider {
		DbTagAssociation					GetAlbumTagAssociation( long albumId, long tagId );

		IDataProviderList<DbTagAssociation>	GetArtistTagList( long artistId, eTagGroup tagGroup );
		IDataProviderList<DbTagAssociation>	GetAlbumTagList( long albumId, eTagGroup tagGroup );
		IDataProviderList<DbTagAssociation>	GetTagList( eTagGroup tagGroup, long tagId );

		void								AddAssociation( DbTagAssociation item );
		void								RemoveAssociation( long tagId );
	}
}
