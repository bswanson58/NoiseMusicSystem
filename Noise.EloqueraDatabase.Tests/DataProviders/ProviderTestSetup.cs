using FluentAssertions;
using Moq;
using Noise.AppSupport;
using Noise.BlobStorage.BlobStore;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	Mock<ILog>					DummyLog { get; private set; }
		public	IIoc						IocProvider { get; private set; }
		public	DatabaseConfiguration		DatabaseConfiguration { get; private set; }
		public	IBlobStorageManager			BlobStorageManager { get; private set; }
		public	IBlobStorageResolver		BlobResolver { get; private set; }
		public	IDatabaseFactory			DatabaseFactory { get; private set; }
		public	IEloqueraManager			DatabaseManager { get; private set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
				
			DatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };

			IocProvider = new IocProvider();
			BlobResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager( BlobResolver );
			DatabaseFactory = new EloqueraDatabaseFactory( BlobStorageManager, IocProvider, DatabaseConfiguration );
			DatabaseManager = new DatabaseManager( DatabaseFactory );

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
