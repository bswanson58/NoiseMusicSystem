using System;
using GDataDB;
using Microsoft.Practices.Unity;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataExchange {
	internal class CloudSyncManager : ICloudSyncManager {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private string						mLoginName;
		private string						mLoginPassword;
		private IDatabaseClient				mCloudClient;
		private IDatabase					mCloudDb;

		public CloudSyncManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public bool InitializeCloudSync() {
			var retValue = false;
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.UseCloud ) {
					mLoginName = configuration.LoginName;
					mLoginPassword = configuration.LoginPassword;

					if(!string.IsNullOrWhiteSpace( mLoginName )) {
						try {
							mCloudClient = new DatabaseClient( mLoginName, mLoginPassword );
							mCloudDb = mCloudClient.GetDatabase( Constants.CloudDatabaseName );
							if( mCloudDb == null ) {
								mCloudClient.CreateDatabase( Constants.CloudDatabaseName );
							}
							else {
								UpdateFromCloud( configuration.LastSequence );
							}

							retValue = true;
						}
						catch( Exception ex ) {
							mLog.LogException( "Exception - CloudSyncManager.InitializeCloudSync: ", ex );
						}
					}
				}
				else {
					retValue = true;
				}
			}

			return( retValue );
		}

		private void UpdateFromCloud( long lastSequence ) {
			var seqnTable = mCloudDb.GetTable<CloudSyncEntry>( Constants.CloudSyncTable );

			if( seqnTable != null ) {
				
			}
			else {
				mCloudDb.CreateTable<CloudSyncEntry>( Constants.CloudSyncTable );
			}
		}
	}
}
