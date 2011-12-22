using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagAssociationProvider {
		DataProviderList<DbTagAssociation>	GetArtistTagList( long artistId, eTagGroup tagGroup );
		DataProviderList<DbTagAssociation>	GetAlbumTagList( long albumId, eTagGroup tagGroup );
		DataProviderList<DbTagAssociation>	GetTagList( eTagGroup tagGroup, long tagId );

		void								AddAssociation( DbTagAssociation item );
		void								RemoveAssociation( long tagId );
	}
}
