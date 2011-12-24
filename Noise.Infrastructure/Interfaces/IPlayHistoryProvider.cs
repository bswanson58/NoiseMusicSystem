using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayHistoryProvider {
		void							AddPlayHistory( DbPlayHistory playHistory );
		void							DeletePlayHistory( DbPlayHistory playHistory );

		DataProviderList<DbPlayHistory>	GetPlayHistoryList();
		DataUpdateShell<DbPlayHistory>	GetPlayHistoryForUpdate( long playListId );
	}
}
