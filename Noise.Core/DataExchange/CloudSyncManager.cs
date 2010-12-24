﻿using System;
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
		private readonly IUnityContainer		mContainer;
		private readonly ILog					mLog;
		private string							mLoginName;
		private string							mLoginPassword;
		private IDatabaseClient					mCloudClient;
		private IDatabase						mCloudDb;
		private bool							mMaintainSynchronization;

		public	ObjectTypes						SynchronizeTypes { get; set; }

		private readonly AsyncCommand<object>						mSyncWithCloud;
		private readonly AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;

		[ImportMany( typeof( ICloudSyncProvider ))]
		public IEnumerable<ICloudSyncProvider>	SyncProviders;

		public CloudSyncManager( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			mSyncWithCloud = new AsyncCommand<object>( OnCloudSync );
			GlobalCommands.SynchronizeFromCloud.RegisterCommand( mSyncWithCloud );

			mSetFavoriteCommand = new AsyncCommand<SetFavoriteCommandArgs>( OnSetFavorite );
			GlobalCommands.SetFavorite.RegisterCommand( mSetFavoriteCommand );
		}

		public bool MaintainSynchronization {
			get{ return( mMaintainSynchronization ); }
			set {
				if( mMaintainSynchronization != value ) {
					mMaintainSynchronization = value;

					if( mMaintainSynchronization ) {

						UpdateFromCloud();
					}
				}
			}
		}

		public bool InitializeCloudSync( string loginName, string password ) {
			mLoginName = loginName;
			mLoginPassword = password;

			return( true );
		}

		private IDatabase CloudDatabase {
			get {
				var retValue = mCloudDb;

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

				return( retValue );
			}
		}

		private void OnCloudSync( object unused ) {
			UpdateFromCloud();

			try {
				var seqnId = ReserveSeqnId();

				SyncFavorites( seqnId );

				UpdateCloudSeqn( seqnId );
				UpdateSeqnId( seqnId );
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - CloudSyncManager:CreateSynchronization: ", ex );
			}
		}

		private void SyncFavorites( long seqnId ) {
			var noiseManager = mContainer.Resolve<INoiseManager>();

			using( var favoriteList = noiseManager.DataProvider.GetFavoriteArtists()) {
				foreach( var artist in favoriteList.List ) {
					var item = new ExportFavorite( noiseManager.DataProvider.DatabaseId, artist.Name, artist.IsFavorite ) { SequenceId = seqnId };

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}

			using( var favoriteList = noiseManager.DataProvider.GetFavoriteAlbums()) {
				foreach( var album in favoriteList.List ) {
					var result = noiseManager.DataProvider.Find( album.DbId );
					var item = new ExportFavorite( result, seqnId );

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}

			using( var favoriteList = noiseManager.DataProvider.GetFavoriteTracks()) {
				foreach( var track in favoriteList.List ) {
					var result = noiseManager.DataProvider.Find( track.DbId );
					var item = new ExportFavorite( result, seqnId );

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}
		}

		private void UpdateFromCloud() {
			try {
				var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
				var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );
				var cloud = CloudDatabase;

				if(( configuration != null ) &&
				   ( cloud != null ) &&
				   ( SyncProviders != null )) {
					var seqnTable = CloudDatabase.GetTable<CloudSyncEntry>( Constants.CloudSyncTable ) ??
									CloudDatabase.CreateTable<CloudSyncEntry>( Constants.CloudSyncTable );

					if( seqnTable != null ) {
						CloudSyncEntry	lastEntry = null;
						var entryList = ( from CloudSyncEntry e in seqnTable.AsQueryable()
										  where e.SequenceNumber > configuration.LastSequence
										  orderby e.SequenceNumber descending
										  select e ).ToList();
						if( entryList.Count > 0 ) {
							lastEntry = entryList[0];
						}

						if( lastEntry != null ) {
							foreach( var provider in SyncProviders ) {
								provider.UpdateFromCloud( configuration.LastSequence, lastEntry.SequenceNumber );
							}

							UpdateSeqnId( lastEntry.SequenceNumber );
						}
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - UpdateFromCloud:", ex );
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			if( mMaintainSynchronization ) { 
				try {
					UpdateFromCloud();

					var noiseManager = mContainer.Resolve<INoiseManager>();
					var seqnId = ReserveSeqnId();

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites ) ) {
							provider.UpdateToCloud( new ExportFavorite( noiseManager.DataProvider.Find( args.ItemId ), seqnId ));
						}
					}

					UpdateCloudSeqn( seqnId );
					UpdateSeqnId( seqnId );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - CloudSyncManager:SetFavorite: ", ex );
				}
			}
		}

		private long ReserveSeqnId() {
			var retValue = 0L;
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				retValue = configuration.LastSequence + 1;
			}

			return( retValue );
		}

		private void UpdateSeqnId( long seqnId ) {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				configuration.LastSequence = seqnId;

				systemConfig.Save( configuration );
			}
		}

		private void UpdateCloudSeqn( long seqnId ) {
			var seqnTable = CloudDatabase.GetTable<CloudSyncEntry>( Constants.CloudSyncTable ) ??
							CloudDatabase.CreateTable<CloudSyncEntry>( Constants.CloudSyncTable );

			if( seqnTable != null ) {
				seqnTable.Add( new CloudSyncEntry { SequenceNumber = seqnId });
			}
		}
	}
}
