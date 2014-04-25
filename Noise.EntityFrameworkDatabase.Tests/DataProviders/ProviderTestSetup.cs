using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	const string				cTestingDirectory = @"D:\Noise Testing";
		public	const string				cDatabaseName = "Integration Test Database";

		public	Mock<ILog>					DummyLog { get; private set; }
		public	IBlobStorageResolver		BlobStorageResolver { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }
		public	Mock<ILibraryConfiguration>	LibraryConfiguration { get; private set; }
		public	LibraryConfiguration		DatabaseConfiguration { get; private set; }
		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	Mock<IDatabaseInfo>			DatabaseInfo { get; private set; }
		public	IDatabaseInitializeStrategy	InitializeStrategy { get; private set; }
		public	IContextProvider			ContextProvider { get; private set; }
		public Mock<IEventAggregator>		EventAggregator { get; set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;

			DatabaseInfo = new Mock<IDatabaseInfo>();
			DatabaseInfo.Setup( m => m.DatabaseId ).Returns( 12345L );

			DatabaseConfiguration = new LibraryConfiguration { DatabaseName = cDatabaseName, LibraryId = 12345 };
			DatabaseConfiguration.SetConfigurationPath( cTestingDirectory );

			LibraryConfiguration = new Mock<ILibraryConfiguration>();
			LibraryConfiguration.Setup( f => f.Current ).Returns( DatabaseConfiguration );

			EventAggregator = new Mock<IEventAggregator>();

			InitializeStrategy = new TestDatabaseInitializer();

			BlobStorageResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager();
			BlobStorageManager.SetResolver( BlobStorageResolver );
			ContextProvider = new ContextProvider( LibraryConfiguration.Object, BlobStorageManager, BlobStorageResolver );

			var manager = new EntityFrameworkDatabaseManager( EventAggregator.Object, LibraryConfiguration.Object,
															  InitializeStrategy, DatabaseInfo.Object, ContextProvider );

			var	initialized = manager.Initialize();
			Assert.IsTrue( initialized );

			manager.Handle( new Events.LibraryChanged());
			DatabaseManager = manager;
		}
	}
}
