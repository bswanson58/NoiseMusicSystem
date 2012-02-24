using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IInternetStreamProvider {
		void								AddStream( DbInternetStream stream );
		void								DeleteStream( DbInternetStream stream );

		DbInternetStream					GetStream( long streamId );
		IDataProviderList<DbInternetStream>	GetStreamList();
		IDataUpdateShell<DbInternetStream>	GetStreamForUpdate( long streamId );
	}
}
