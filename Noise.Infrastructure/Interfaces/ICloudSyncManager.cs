using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ICloudSyncManager {
		bool			InitializeCloudSync();

		void			CreateSynchronization();
		bool			MaintainSynchronization { get; set; }
		ObjectTypes		SynchronizeTypes { get; set; }
	}
}
