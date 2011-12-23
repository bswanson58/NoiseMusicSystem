using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDomainSearchProvider {
		DataFindResults		Find( string artist, string album, string track );
		DataFindResults		Find( long itemId );
	}
}
