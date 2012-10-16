using Caliburn.Micro;
using FluentAssertions;
using Moq;
using Noise.AppSupport;
using Noise.BlobStorage.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	Mock<ILog>					DummyLog { get; private set; }
		public	Mock<IEventAggregator>		EventAggregator { get; private set; }
		public	IIoc						IocProvider { get; private set; }
		public	ILibraryConfiguration		LibraryConfiguration { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }
		public	IBlobStorageResolver		BlobResolver { get; private set; }
		public	IDatabaseFactory			DatabaseFactory { get; private set; }
		public	IEloqueraManager			DatabaseManager { get; private set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
				
			var databaseConfig = new LibraryConfiguration { DatabaseName = "Integration Test Database" };

			var configMoq = new Mock<ILibraryConfiguration>();
			configMoq.Setup( m => m.Current ).Returns( databaseConfig );
			LibraryConfiguration = configMoq.Object;

			EventAggregator = new Mock<IEventAggregator>();
			IocProvider = new IocProvider();
			BlobResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager();
			BlobStorageManager.SetResolver( BlobResolver );
			DatabaseFactory = new EloqueraDatabaseFactory( EventAggregator.Object, BlobStorageManager, BlobResolver, IocProvider, LibraryConfiguration );

			var eventAggreagtor = new Mock<IEventAggregator>();

			DatabaseManager = new DatabaseManager( eventAggreagtor.Object, LibraryConfiguration, DatabaseFactory );

			if( DatabaseManager.Initialize()) {
				using( var database = DatabaseManager.CreateDatabase()) {
					database.Database.OpenWithCreateDatabase();
				}
			}
		}

		public void Teardown() {
			DatabaseManager.ReservedDatabaseCount.Should().Be( 0 );

			using( var database = DatabaseManager.CreateDatabase()) {
				database.Database.DeleteDatabase();
			}
		}
	}
}
