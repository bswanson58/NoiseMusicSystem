using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDiscographyProvider {
		void									AddDiscography( DbDiscographyRelease release );
		void									RemoveDiscography( DbDiscographyRelease release );

		IDataProviderList<DbDiscographyRelease>	GetDiscography( long artistId );
	}
}
