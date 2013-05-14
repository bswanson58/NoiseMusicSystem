using Moq;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Noise.RavenDatabase.Tests.DataProviders {
	public class CommonTestSetup {
		private IDocumentStore		mDatabase;

		public	Mock<ILog>			DummyLog { get; private set; }
		public	Mock<IDbFactory>	DatabaseFactory { get; private set; }

		public void FixtureSetup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
		}

		public void Setup() {
			mDatabase = new EmbeddableDocumentStore { DataDirectory = "Test Database", RunInMemory = true };
			// Wait for all operations to complete before each query.
			mDatabase.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;
			mDatabase.Initialize();

			DatabaseFactory = new Mock<IDbFactory>();
			DatabaseFactory.Setup( o => o.GetLibraryDatabase()).Returns( mDatabase );
		}

		public void Teardown() {
			if( mDatabase != null ) {
				mDatabase.Dispose();

				DatabaseFactory = null;
			}
		}
	}
}
