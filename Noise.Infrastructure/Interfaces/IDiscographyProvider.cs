using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDiscographyProvider {
		DataProviderList<DbDiscographyRelease>	GetDiscography( long artistId );
	}
}
