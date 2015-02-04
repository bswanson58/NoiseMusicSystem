using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Moq;
using Noise.BlobStorage.BlobStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.RavenDatabase.Interfaces;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Noise.RavenDatabase.Tests.DataProviders {
	public class CommonTestSetup {
		public	IBlobStorageManager		BlobStorageManager { get; private set; }
		public	IBlobStorageResolver	BlobResolver { get; private set; }
		private IDocumentStore			mDatabase;
		private Subject<bool>			mDatabaseClosedSubject;
		public IObservable<bool>		DatabaseClosed { get { return ( mDatabaseClosedSubject.AsObservable()); } }

		public	Mock<INoiseLog>			DummyLog { get; private set; }
		public	Mock<IDbFactory>		DatabaseFactory { get; private set; }

		public void FixtureSetup() {
			DummyLog = new Mock<INoiseLog>();
			NoiseLogger.Current = DummyLog.Object;

			mDatabaseClosedSubject = new Subject<bool>();
		}

		public void Setup() {
			mDatabase = new EmbeddableDocumentStore { DataDirectory = "Test Database", RunInMemory = true };
			mDatabase.Initialize();
			IndexCreation.CreateIndexes( typeof( RavenDatabaseModule ).Assembly, mDatabase );

			BlobResolver = new BlobStorageResolver();
			BlobStorageManager = new BlobStorageManager();
			BlobStorageManager.SetResolver( BlobResolver );

			DatabaseFactory = new Mock<IDbFactory>();
			DatabaseFactory.Setup( o => o.GetLibraryDatabase()).Returns( mDatabase );
			DatabaseFactory.Setup( o => o.GetBlobStorage()).Returns( BlobStorageManager.GetStorage());
			DatabaseFactory.Setup( o => o.DatabaseClosed ).Returns( DatabaseClosed );
		}

		public void Teardown() {
			if( mDatabase != null ) {
				mDatabase.Dispose();

				DatabaseFactory = null;
			}
		}
	}
}
