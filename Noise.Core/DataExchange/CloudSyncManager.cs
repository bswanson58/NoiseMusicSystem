using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using GDataDB;
using GDataDB.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataExchange {
	internal class CloudSyncManager : ICloudSyncManager {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;
		private bool						mUseCloud;
		private string						mLoginName;
		private string						mLoginPassword;
		private IDatabaseClient				mCloudClient;
		private IDatabase					mCloudDb;
		private bool						mMaintainSynchronization;

		private readonly AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;

		public	ObjectTypes					SynchronizeTypes { get; set; }

		[ImportMany( typeof( ICloudSyncProvider ))]
		public IEnumerable<ICloudSyncProvider>	SyncProviders;

		public CloudSyncManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
		}

		public bool MaintainSynchronization {
			get{ return( mMaintainSynchronization ); }
			set {
				if( mMaintainSynchronization != value ) {
					mMaintainSynchronization = value;

					if( mMaintainSynchronization ) {
						GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );

						UpdateFromCloud();
					}
					else {
						GlobalCommands.SetFavorite.UnregisterCommand( mSetFavoriteCommand );
					}
				}
			}
		}

		public bool InitializeCloudSync() {
			var retValue = false;
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				if( configuration.UseCloud ) {
					mUseCloud = true;
					mLoginName = configuration.LoginName;
					mLoginPassword = configuration.LoginPassword;
				}
				else {
					retValue = true;
				}
			}

			return( retValue );
		}

		private IDatabase CloudDatabase {
			get {
				var retValue = mCloudDb;

				if( mUseCloud ) {
					if( mCloudDb == null ) {
						try {
							if( mCloudClient == null ) {
								mCloudClient = new DatabaseClient( mLoginName, mLoginPassword );
							}

							mCloudDb = mCloudClient.GetDatabase( Constants.CloudDatabaseName ) ??
									   mCloudClient.CreateDatabase( Constants.CloudDatabaseName );

							retValue = mCloudDb;
						}
						catch( Exception ex ) {
							mLog.LogException( "Exception - CloudSymcManager:Opening cloud database: ", ex );
						}
					}

					if( SyncProviders == null ) {
						var catalog = new DirectoryCatalog(  @".\" );
						var container = new CompositionContainer( catalog );
						container.ComposeParts( this );

						if( SyncProviders != null ) {
							foreach( var provider in SyncProviders ) {
								provider.Initialize( mContainer, mCloudDb );
							}
						}
					}
				}

				return( retValue );
			}
		}

		public void CreateSynchronization() {
			
		}

		private void UpdateFromCloud() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				var seqnTable = CloudDatabase.GetTable<CloudSyncEntry>( Constants.CloudSyncTable ) ??
								CloudDatabase.CreateTable<CloudSyncEntry>( Constants.CloudSyncTable );

				if( seqnTable != null ) {
					var lastEntry = ( from CloudSyncEntry e in seqnTable.AsQueryable()
									  where e.SequenceNumber > configuration.LastSequence
									  orderby e.SequenceNumber descending select e ).FirstOrDefault();
					if( lastEntry != null ) {
						foreach( var provider in SyncProviders ) {
							provider.UpdateFromCloud( configuration.LastSequence, lastEntry.SequenceNumber );
						}

						seqnTable.Add( new CloudSyncEntry { SequenceNumber = lastEntry.SequenceNumber + 1 });
					}
				}
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			UpdateFromCloud();

			var noiseManager = mContainer.Resolve<INoiseManager>();

			foreach( var provider in SyncProviders ) {
				if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
					var item = noiseManager.DataProvider.Find( args.ItemId );

//					provider.UpdateToCloud( new ExportBase( noiseManager.DataProvider.DatabaseId ));
				}
			}
		}
	}
}
