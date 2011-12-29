using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDbBaseProvider {
		DbBase		GetItem( long itemId );

		long		DatabaseInstanceId();
	}
}
