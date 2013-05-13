using Moq;
using NUnit.Framework;
using Noise.BaseDatabase.Tests.DataProviders;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.DataProviders;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Noise.RavenDatabase.Tests.DataProviders {
	[TestFixture]
	public class ArtistProviderTests : BaseArtistProviderTests {
		public	Mock<ILog>			DummyLog { get; private set; }
		private IDocumentStore		mDatabase;
		private Mock<IDbFactory>	mDatabaseFactory;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			DummyLog = new Mock<ILog>();
			NoiseLogger.Current = DummyLog.Object;
		}

		[SetUp]
		public void Setup() {
			mDatabase = new EmbeddableDocumentStore { DataDirectory = "Test Database", RunInMemory = true };
			// Wait for all operations to complete before each query.
			mDatabase.Conventions.DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites;
			mDatabase.Initialize();

			mDatabaseFactory = new Mock<IDbFactory>();
			mDatabaseFactory.Setup( o => o.GetLibraryDatabase()).Returns( mDatabase );
		}

		[TearDown]
		public void Teardown() {
			if( mDatabase != null ) {
				mDatabase.Dispose();

				mDatabaseFactory = null;
			}
		}

		protected override IArtistProvider CreateSut() {
			return( new ArtistProvider( mDatabaseFactory.Object ));
		}
	}
}
