﻿using Moq;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	Mock<ILog>					DummyLog { get; private set; }
		public	IBlobStorageResolver		BlobStorageResolver { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }			
		public	DatabaseConfiguration		DatabaseConfiguration { get; private set; }
		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	Mock<IDatabaseInfo>			DatabaseInfo { get; private set; }
		public	IDatabaseInitializeStrategy	InitializeStrategy { get; private set; }
		public	IContextProvider			ContextProvider { get; private set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
				
			DatabaseInfo = new Mock<IDatabaseInfo>();
			DatabaseInfo.Setup( m => m.DatabaseId ).Returns( 12345L );

			DatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };
			InitializeStrategy = new TestDatabaseInitializer();

			BlobStorageResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager( BlobStorageResolver );
			ContextProvider = new ContextProvider( BlobStorageManager );

			DatabaseManager = new EntityFrameworkDatabaseManager( InitializeStrategy, DatabaseInfo.Object, ContextProvider );

			var	initialized = DatabaseManager.Initialize();
			Assert.IsTrue( initialized );
		}
	}
}
