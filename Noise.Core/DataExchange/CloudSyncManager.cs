﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GDataDB;
using GDataDB.Linq;
using Noise.Core.DataExchange.Dto;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using IDatabase = GDataDB.IDatabase;

namespace Noise.Core.DataExchange {
	internal class CloudSyncManager : ICloudSyncManager {
		private readonly IIoc				mComponentCreator;
		private readonly IDataProvider		mDataProvider;
		private string						mLoginName;
		private string						mLoginPassword;
		private IDatabaseClient				mCloudClient;
		private IDatabase					mCloudDb;
		private bool						mMaintainSynchronization;

		public	ObjectTypes					SynchronizeTypes { get; set; }

		private readonly AsyncCommand<object>						mSyncWithCloud;
		private readonly AsyncCommand<SetFavoriteCommandArgs>		mSetFavoriteCommand;

		[ImportMany( typeof( ICloudSyncProvider ))]
		public IEnumerable<ICloudSyncProvider>	SyncProviders;

		public CloudSyncManager( IIoc componentCreator, IDataProvider dataProvider ) {
			mComponentCreator = componentCreator;
			mDataProvider = dataProvider;
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
						NoiseLogger.Current.LogException( "Exception - CloudSymcManager:Opening cloud database: ", ex );
					}
				}

				if( SyncProviders == null ) {
					mComponentCreator.ComposeParts( this );

					if( SyncProviders != null ) {
						foreach( var provider in SyncProviders ) {
							provider.Initialize( mDataProvider, mCloudDb );
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
				SyncStreams( seqnId );

				UpdateCloudSeqn( seqnId );
				UpdateSeqnId( seqnId );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - CloudSyncManager:CreateSynchronization: ", ex );
			}
		}

		private void SyncFavorites( long seqnId ) {
			using( var favoriteList = mDataProvider.GetFavoriteArtists()) {
				foreach( var artist in favoriteList.List ) {
					var item = new ExportFavorite( mDataProvider.DatabaseId, artist.Name, artist.IsFavorite ) { SequenceId = seqnId };

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}

			using( var favoriteList = mDataProvider.GetFavoriteAlbums()) {
				foreach( var album in favoriteList.List ) {
					var result = mDataProvider.Find( album.DbId );
					var item = new ExportFavorite( result, seqnId );

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}

			using( var favoriteList = mDataProvider.GetFavoriteTracks()) {
				foreach( var track in favoriteList.List ) {
					var result = mDataProvider.Find( track.DbId );
					var item = new ExportFavorite( result, seqnId );

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}
		}

		private void SyncStreams( long seqnId ) {
			using( var streamList = mDataProvider.GetStreamList()) {
				foreach( var stream in streamList.List ) {
					var item = new ExportStream( mDataProvider.DatabaseId, stream.Name, stream.Description, stream.Url, stream.IsPlaylistWrapped, stream.Website ) { SequenceId = seqnId };

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Streams )) {
							provider.UpdateToCloud( item );
						}
					}
				}
			}
		}

		private void UpdateFromCloud() {
			try {
				var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );
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
				NoiseLogger.Current.LogException( "Exception - UpdateFromCloud:", ex );
			}
		}

		private void OnSetFavorite( SetFavoriteCommandArgs args ) {
			if( mMaintainSynchronization ) { 
				try {
					UpdateFromCloud();

					var seqnId = ReserveSeqnId();

					foreach( var provider in SyncProviders ) {
						if( provider.SyncTypes.HasFlag( ObjectTypes.Favorites ) ) {
							provider.UpdateToCloud( new ExportFavorite( mDataProvider.Find( args.ItemId ), seqnId ));
						}
					}

					UpdateCloudSeqn( seqnId );
					UpdateSeqnId( seqnId );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - CloudSyncManager:SetFavorite: ", ex );
				}
			}
		}

		private static long ReserveSeqnId() {
			var retValue = 0L;
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				retValue = configuration.LastSequence + 1;
			}

			return( retValue );
		}

		private static void UpdateSeqnId( long seqnId ) {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<CloudSyncConfiguration>( CloudSyncConfiguration.SectionName );

			if( configuration != null ) {
				configuration.LastSequence = seqnId;

				NoiseSystemConfiguration.Current.Save( configuration );
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
