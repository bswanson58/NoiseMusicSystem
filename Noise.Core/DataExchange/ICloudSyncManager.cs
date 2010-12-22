using Noise.Infrastructure.Dto;

namespace Noise.Core.DataExchange {
	public interface ICloudSyncManager {
		bool			InitializeCloudSync();

		void			CreateSynchronization();
		bool			MaintainSynchronization { get; set; }
		ObjectTypes		SynchronizeTypes { get; set; }
	}
}
