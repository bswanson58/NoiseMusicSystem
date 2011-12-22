using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IInternetStreamProvider {
		void								AddStream( DbInternetStream stream );
		void								DeleteStream( DbInternetStream stream );

		DbInternetStream					GetStream( long streamId );
		DataProviderList<DbInternetStream>	GetStreamList();
		DataUpdateShell<DbInternetStream>	GetStreamForUpdate( long streamId );
	}
}
