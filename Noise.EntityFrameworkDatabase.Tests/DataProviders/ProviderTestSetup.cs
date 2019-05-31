using Caliburn.Micro;
using Moq;
using Noise.EntityFrameworkDatabase.Logging;
using NUnit.Framework;
using Noise.BlobStorage.BlobStore;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	const string				cTestingDirectory = @"D:\Noise Testing";
		public	const string				cDatabaseName = "Integration Test Database";

		public	IBlobStorageResolver		BlobStorageResolver { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }
        public  IBlobStorageProvider        BlobStorageProvider { get; private set; }
		public	Mock<ILibraryConfiguration>	LibraryConfiguration { get; private set; }
		public	LibraryConfiguration		DatabaseConfiguration { get; private set; }
		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	Mock<IDatabaseInfo>			DatabaseInfo { get; private set; }
		public	IDatabaseInitializeStrategy	InitializeStrategy { get; private set; }
		public	IContextProvider			ContextProvider { get; private set; }
		public	Mock<IEventAggregator>		EventAggregator { get; set; }
		private Mock<ILogDatabase>			Log { get; set; }

		public void Setup() {
			Log = new Mock<ILogDatabase>();

			DatabaseInfo = new Mock<IDatabaseInfo>();
			DatabaseInfo.Setup( m => m.DatabaseId ).Returns( 12345L );

			DatabaseConfiguration = new LibraryConfiguration { DatabaseName = cDatabaseName, LibraryId = 12345 };
			DatabaseConfiguration.SetConfigurationPath( cTestingDirectory );

			LibraryConfiguration = new Mock<ILibraryConfiguration>();
			LibraryConfiguration.Setup( f => f.Current ).Returns( DatabaseConfiguration );

			EventAggregator = new Mock<IEventAggregator>();

			InitializeStrategy = new TestDatabaseInitializer();

			BlobStorageResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager( new Mock<INoiseLog>().Object );
			BlobStorageManager.SetResolver( BlobStorageResolver );
            BlobStorageProvider = new BlobStorageProvider( LibraryConfiguration.Object, EventAggregator.Object, null, null, () => BlobStorageManager, BlobStorageResolver );
			ContextProvider = new ContextProvider( LibraryConfiguration.Object, Log.Object, BlobStorageProvider );

			var manager = new EntityFrameworkDatabaseManager( EventAggregator.Object, Log.Object, LibraryConfiguration.Object,
															  InitializeStrategy, DatabaseInfo.Object, ContextProvider );

			var	initialized = manager.Initialize();
			Assert.IsTrue( initialized );

			manager.Handle( new Events.LibraryChanged());
			DatabaseManager = manager;
		}
	}
}
