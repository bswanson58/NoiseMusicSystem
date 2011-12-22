using System.Linq;
using GDataDB.Linq;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using IDatabase = GDataDB.IDatabase;

namespace Noise.Core.DataExchange {
	public class CloudSyncStreams : ICloudSyncProvider {
		private readonly IInternetStreamProvider	mStreamProvider;
		private IDatabase							mCloudDatabase;

		public CloudSyncStreams( IInternetStreamProvider streamProvider ) {
			mStreamProvider = streamProvider;
		}

		public bool Initialize( IDatabase cloudDatabase ) {
			mCloudDatabase = cloudDatabase;

			return( true );
		}

		public ObjectTypes SyncTypes {
			get{ return( ObjectTypes.Streams ); }
		}

		public void UpdateFromCloud( long fromSeqn, long toSeqn ) {
			var streamTable = mCloudDatabase.GetTable<ExportStream>( Constants.CloudSyncStreamsTable ) ??
							  mCloudDatabase.CreateTable<ExportStream>( Constants.CloudSyncStreamsTable );
			var cloudStreams = from ExportStream e in streamTable.AsQueryable() 
							   where e.SequenceId > fromSeqn && e.SequenceId <= toSeqn select e;
			var updateCount = 0;

			foreach( var stream in cloudStreams ) {
				using( var streamList = mStreamProvider.GetStreamList()) {
					var streamName = stream.Stream;
					var dbStream = ( from DbInternetStream str in streamList.List where str.Name == streamName select str ).FirstOrDefault();

					if( dbStream != null ) {
						using( var updateStream = mStreamProvider.GetStreamForUpdate( dbStream.DbId )) {
							updateStream.Item.Description = stream.Description;
							updateStream.Item.IsPlaylistWrapped = stream.IsPlaylistWrapped;
							updateStream.Item.Url = stream.Url;
							updateStream.Item.Website = stream.Website;

							updateStream.Update();
						}
					}
					else {
						dbStream = new DbInternetStream{ Description = stream.Description, IsPlaylistWrapped = stream.IsPlaylistWrapped,
														 Url = stream.Url, Website = stream.Website };

						mStreamProvider.AddStream( dbStream );
						updateCount++;
					}
				}
			}

			if( updateCount > 0 ) {
				NoiseLogger.Current.LogInfo( string.Format( "CloudSyncStreams - UpdateFromCloud: {0} item(s).", updateCount ));
			}
		}

		public void UpdateToCloud( ExportBase item ) {
			if( item is ExportStream ) {
				var streamExport = item as ExportStream;
				var streamDb = mCloudDatabase.GetTable<ExportStream>( Constants.CloudSyncStreamsTable ) ??
							   mCloudDatabase.CreateTable<ExportStream>( Constants.CloudSyncStreamsTable );
				if( streamDb != null ) {
					var row = streamDb.FindAll().Where( e => e.Element.Compare( item )).FirstOrDefault();

					if( row != null ) {
						row.Element.Description = streamExport.Description;
						row.Element.IsPlaylistWrapped = streamExport.IsPlaylistWrapped;
						row.Element.Url = streamExport.Url;
						row.Element.Website = streamExport.Website;
						row.Element.SequenceId = streamExport.SequenceId;
						row.Element.OriginDb = streamExport.OriginDb;

						row.Update();
					}
					else {
						streamDb.Add( streamExport );
					}
				}

				NoiseLogger.Current.LogInfo( string.Format( "Updated stream to cloud: {0}", item.Stream ));
			}
		}
	}
}
