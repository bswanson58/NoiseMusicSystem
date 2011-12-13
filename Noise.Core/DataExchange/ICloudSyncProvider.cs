using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	public interface ICloudSyncProvider {
		bool		Initialize( IDataProvider dataProvider, GDataDB.IDatabase database );

		ObjectTypes	SyncTypes { get; }

		void		UpdateFromCloud( long fromSeqn, long toSeqn );
		void		UpdateToCloud( ExportBase item );
	}
}
