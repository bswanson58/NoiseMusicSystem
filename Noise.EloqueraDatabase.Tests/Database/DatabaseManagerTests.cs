using System.Collections.Generic;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using Noise.EloqueraDatabase.Database;
using Noise.EloqueraDatabase.Interfaces;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.EloqueraDatabase.Tests.Database {
	[TestFixture]
	public class DatabaseManagerTests {
		private const string				cDatabaseInstanceName = "whatever";

		private Mock<IEventAggregator>		mEventAggregator;
		private Mock<ILibraryConfiguration>	mLibraryConfiguration;
		private Mock<IDatabaseFactory>		mDatabaseFactory;
		private Mock<IDatabase>				mDatabase;
		private Mock<IBlobStorage>			mBlobStorage;
		private	bool						mDatabaseOpenCalled;

		[TestFixtureSetUp]
		public void FixtureSetup() {
			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[SetUp]
		public void Setup() {
			mDatabaseOpenCalled = false;

			mEventAggregator = new Mock<IEventAggregator>();
			mLibraryConfiguration = new Mock<ILibraryConfiguration>();
			mDatabaseFactory = new Mock<IDatabaseFactory>();
			mBlobStorage = new Mock<IBlobStorage>();
			mDatabase = new Mock<IDatabase>();

			mDatabase.Setup( s => s.DatabaseId ).Returns( cDatabaseInstanceName );
			mDatabase.Setup( s => s.InitializeDatabase()).Returns( true );
			mDatabase.Setup( s => s.BlobStorage ).Returns( mBlobStorage.Object );
			mDatabase.Setup( s => s.OpenWithCreateDatabase()).Returns( true ).Callback( () => mDatabaseOpenCalled = true );
			mDatabase.Setup( s => s.InitializeAndOpenDatabase()).Returns( true ).Callback( () => mDatabaseOpenCalled = true );

			mDatabaseFactory.Setup( m => m.GetDatabaseInstance()).Returns( mDatabase.Object );
		}

		private DatabaseManager CreateSut() {
			return new DatabaseManager( mEventAggregator.Object, mLibraryConfiguration.Object, mDatabaseFactory.Object );
		}

		[Test]
		public void CanInitializeDatabaseManager() {
			var sut = CreateSut();

			Assert.IsTrue( sut.Initialize());
		}

		[Test]
		public void CanCreateDatabaseInstance() {
			var sut = CreateSut();

			Assert.IsTrue( sut.Initialize());
			var reserved = sut.ReserveDatabase();

			Assert.AreEqual( mDatabase.Object, reserved );
			Assert.AreEqual( mBlobStorage.Object, reserved.BlobStorage );
			Assert.IsTrue( mDatabaseOpenCalled );
		}

		[Test]
		public void CanCreateMultipleDatabases() {
			var database1 = new Mock<IDatabase>();
			var database2 = new Mock<IDatabase>();
			var dbList = new List<IDatabase> { database1.Object, database2.Object };
			var dbEnum = dbList.GetEnumerator();

			database1.Setup( m => m.DatabaseId ).Returns( "database 1" );
			database2.Setup( m => m.DatabaseId ).Returns( "database 2" );
		
			dbEnum.MoveNext();
			mDatabaseFactory.Setup( m => m.GetDatabaseInstance()).Returns( () => dbEnum.Current )
																 .Callback( () => { dbEnum.MoveNext(); mDatabaseOpenCalled = true; });

			var sut = CreateSut();
			Assert.IsTrue( sut.Initialize());
			var reserved1 = sut.ReserveDatabase();
			Assert.IsTrue( mDatabaseOpenCalled );

			mDatabaseOpenCalled = false;
			var reserved2 = sut.ReserveDatabase();
			Assert.IsTrue( mDatabaseOpenCalled );

			Assert.AreEqual( database1.Object, reserved1 );
			Assert.AreEqual( database2.Object, reserved2 );
		}

		[Test]
		public void CanReuseDatabaseInstance() {
			var sut  = CreateSut();
			Assert.IsTrue( sut.Initialize());

			Assert.AreEqual( mDatabase.Object, sut.ReserveDatabase());
			sut.FreeDatabase( mDatabase.Object );
			Assert.AreEqual( mDatabase.Object, sut.ReserveDatabase());
			sut.FreeDatabase( mDatabase.Object );

			mDatabaseFactory.Verify( s => s.GetDatabaseInstance(), Times.Once());
		}

		[Test]
		public void CanRetrieveExistingDatabaseInstance() {
			var sut = CreateSut();
			Assert.IsTrue( sut.Initialize());

			var reserved = sut.ReserveDatabase();
			Assert.AreEqual( reserved, sut.GetDatabase( cDatabaseInstanceName ));
		}

		[Test]
		public void CanShutdownDatabaseManager() {
			var sut = CreateSut();
			Assert.IsTrue( sut.Initialize());

			var reserved = sut.ReserveDatabase();
			sut.FreeDatabase( reserved );

			sut.Shutdown();
			mDatabase.Verify( s => s.CloseDatabase());

			Assert.IsNull( sut.ReserveDatabase());
		}
	}
}
