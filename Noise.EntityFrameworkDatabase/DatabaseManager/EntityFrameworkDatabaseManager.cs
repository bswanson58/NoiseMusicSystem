﻿using System;
using System.IO;
using Caliburn.Micro;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.DatabaseManager {
	public class EntityFrameworkDatabaseManager : IDatabaseManager,
												  IHandle<Events.LibraryChanged> {
		private const Int16		cDatabaseVersion = 1;

		private readonly IDatabaseInitializeStrategy	mInitializeStrategy;
		private readonly IDatabaseInfo					mDatabaseInfo;
		private readonly IContextProvider				mContextProvider;

		public EntityFrameworkDatabaseManager( IDatabaseInitializeStrategy initializeStrategy, IDatabaseInfo databaseInfo, IContextProvider contextProvider ) {
			mInitializeStrategy = initializeStrategy;
			mDatabaseInfo = databaseInfo;
			mContextProvider = contextProvider;
		}

		public bool IsOpen {
			get{ return( false ); }
		}

		public void Handle( Events.LibraryChanged args ) {
			
		}

		public bool Initialize() {
			var retValue = false;

			if( mInitializeStrategy != null ) {
				retValue = mInitializeStrategy.InitializeDatabase( mContextProvider.CreateContext());

				if(( retValue ) &&
				   ( mInitializeStrategy.DidCreateDatabase )) {
					mDatabaseInfo.InitializeDatabaseVersion( cDatabaseVersion );
				}
			}

			mContextProvider.BlobStorageManager.Initialize( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), Constants.CompanyName ));
			if(!mContextProvider.BlobStorageManager.IsOpen ) {
				if(!mContextProvider.BlobStorageManager.OpenStorage()) {
					mContextProvider.BlobStorageManager.CreateStorage();

					if(!mContextProvider.BlobStorageManager.OpenStorage()) {
						var ex = new ApplicationException( "EntityFrameworkDatabaseManager:Blob storage could not be created." );

						NoiseLogger.Current.LogException( ex );
						throw( ex );
					}
				}
			}

			return( retValue );
		}

		public void Shutdown() {
		}
	}
}
