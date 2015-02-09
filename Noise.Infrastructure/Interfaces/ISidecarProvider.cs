using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ISidecarProvider {
		void								Add( StorageSidecar sidecar );
		void								Delete( StorageSidecar sidecar );

		StorageSidecar						GetSidecar( long dbid );
		StorageSidecar						GetSidecarForArtist( DbArtist artist );
		StorageSidecar						GetSidecarForAlbum( DbAlbum album );

		IDataProviderList<StorageSidecar>	GetUnreadSidecars();

		IDataUpdateShell<StorageSidecar>	GetSidecarForUpdate( long dbid ); 
	}
}
