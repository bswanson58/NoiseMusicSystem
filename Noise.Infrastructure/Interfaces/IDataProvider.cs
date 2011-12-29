using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataProvider {
		long	DatabaseId { get; }

		DbBase	GetItem( long dbid );
	}
}
