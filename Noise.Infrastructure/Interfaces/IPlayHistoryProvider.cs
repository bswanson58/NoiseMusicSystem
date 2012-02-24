using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayHistoryProvider {
		void							AddPlayHistory( DbPlayHistory playHistory );
		void							DeletePlayHistory( DbPlayHistory playHistory );

		IDataProviderList<DbPlayHistory>	GetPlayHistoryList();
		IDataUpdateShell<DbPlayHistory>		GetPlayHistoryForUpdate( long playListId );
	}
}
