using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ICloudSyncManager {
		bool			InitializeCloudSync( string loginName, string password );

		bool			MaintainSynchronization { get; set; }
		ObjectTypes		SynchronizeTypes { get; set; }
	}
}
