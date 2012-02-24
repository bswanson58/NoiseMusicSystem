using Moq;
using NUnit.Framework;
using Noise.EntityFrameworkDatabase.DatabaseManager;
using Noise.EntityFrameworkDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.EntityFrameworkDatabase.Tests.DataProviders {
	public class ProviderTestSetup {
		public	Mock<ILog>					DummyLog { get; private set; }
		public	DatabaseConfiguration		DatabaseConfiguration { get; private set; }
		public	IDatabaseManager			DatabaseManager { get; private set; }
		public	IDatabaseInitializeStrategy	InitializeStrategy { get; private set; }
		public	IContextProvider			ContextProvider { get; private set; }

		public void Setup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
				
			DatabaseConfiguration = new DatabaseConfiguration { DatabaseName = "Integration Test Database" };
			InitializeStrategy = new TestDatabaseInitializer();
			ContextProvider = new ContextProvider();

			DatabaseManager = new EntityFrameworkDatabaseManager( InitializeStrategy, ContextProvider );

			var	initialized = DatabaseManager.Initialize();
			Assert.IsTrue( initialized );
		}
	}
}
