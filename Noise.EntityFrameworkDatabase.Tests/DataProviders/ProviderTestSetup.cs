using Moq;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	Mock<ILog>					DummyLog { get; private set; }
		public	IBlobStorageResolver		BlobStorageResolver { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }
		public	Mock<ILibraryConfiguration>	LibraryConfiguration { get; private set; }
		public	LibraryConfiguration		DatabaseConfiguration { get; private set; }
		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	Mock<IDatabaseInfo>			DatabaseInfo { get; private set; }
		public	IDatabaseInitializeStrategy	InitializeStrategy { get; private set; }
		public	IContextProvider			ContextProvider { get; private set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
				
			DatabaseInfo = new Mock<IDatabaseInfo>();
			DatabaseInfo.Setup( m => m.DatabaseId ).Returns( 12345L );

			DatabaseConfiguration = new LibraryConfiguration { DatabaseName = "Integration Test Database", LibraryId = 12345 };
			LibraryConfiguration = new Mock<ILibraryConfiguration>();
			LibraryConfiguration.Setup( f => f.Current ).Returns( DatabaseConfiguration );

			InitializeStrategy = new TestDatabaseInitializer();

			BlobStorageResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager();
			BlobStorageManager.SetResolver( BlobStorageResolver );
			ContextProvider = new ContextProvider( LibraryConfiguration.Object, BlobStorageManager );

			DatabaseManager = new EntityFrameworkDatabaseManager( InitializeStrategy, DatabaseInfo.Object, ContextProvider );

			var	initialized = DatabaseManager.Initialize();
			Assert.IsTrue( initialized );
		}
	}
}
