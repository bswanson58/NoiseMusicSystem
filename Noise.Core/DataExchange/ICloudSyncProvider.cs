using Microsoft.Practices.Unity;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataExchange {
	public interface ICloudSyncProvider {
		bool		Initialize( IUnityContainer container, GDataDB.IDatabase database );

		ObjectTypes	SyncTypes { get; }

		void		UpdateFromCloud( long fromSeqn, long toSeqn );
		void		UpdateToCloud( ExportBase item );
	}
}
