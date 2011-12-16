using System.Collections.Generic;
using MbUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Rhino.Mocks;

namespace Noise.Core.Tests.Database {
	public class DatabaseManagerTests {
		[Test]
		[ExpectedArgumentNullException]
		public void DatabaseManagerRequiresDatabaseFactory() {
			new DatabaseManager( null );
		}

		[Test]
		public void CanInitializeDatabaseManager() {
			var databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var database = CreateDatabaseStub( "whatever" );

			databaseFactory.Stub( s => s.GetDatabaseInstance()).Return( database );

			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());
		}

		[Test]
		public void CanCreateDatabaseInstance() {
			var		databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var		database = CreateDatabaseStub( "One" );
			var		blobStorage = MockRepository.GenerateStub<IBlobStorage>();

			databaseFactory.Stub( s => s.GetDatabaseInstance()).Return( database );
			databaseFactory.Stub( s => s.SetBlobStorageInstance( database )).WhenCalled( method => database.BlobStorage = blobStorage );

			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());

			var reserved = dbm.ReserveDatabase();

			Assert.AreEqual( database, reserved );
			Assert.AreEqual( blobStorage, database.BlobStorage );
			Assert.IsTrue( mDatabaseOpenCalled );
		}

		[Test]
		public void CanCreateMultipleDatabases() {
			var databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var database1 = CreateDatabaseStub( "TestInstance1" );
			var database2 = CreateDatabaseStub( "TestInstance2" );
			var dbList = new List<IDatabase> { database1, database2 };
			var dbIndex = 0;

			databaseFactory.Stub( s => s.GetDatabaseInstance())
				.Return( null )
				.WhenCalled( method => { method.ReturnValue = dbList[dbIndex]; dbIndex++; } );


			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());
			var reserved1 = dbm.ReserveDatabase();
			Assert.IsTrue( mDatabaseOpenCalled );

			mDatabaseOpenCalled = false;
			var reserved2 = dbm.ReserveDatabase();
			Assert.IsTrue( mDatabaseOpenCalled );

			Assert.AreEqual( database1, reserved1 );
			Assert.AreEqual( database2, reserved2 );
		}

		[Test]
		public void CanReuseDatabaseInstance() {
			var databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var database = CreateDatabaseStub( "TestInstance" );

			databaseFactory.Stub( s => s.GetDatabaseInstance()).Return( database );

			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());

			Assert.AreEqual( database, dbm.ReserveDatabase());
			dbm.FreeDatabase( database );
			Assert.AreEqual( database, dbm.ReserveDatabase());
			dbm.FreeDatabase( database );

			databaseFactory.AssertWasCalled( s => s.GetDatabaseInstance(), options => options.Repeat.Once());
		}

		[Test]
		public void CanRetrieveExistingDatabaseInstance() {
			const string instanceName = "My Instance";

			var databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var database = CreateDatabaseStub( instanceName );

			databaseFactory.Stub( s => s.GetDatabaseInstance()).Return( database );

			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());

			var reserved = dbm.ReserveDatabase();
			Assert.AreEqual( reserved, dbm.GetDatabase( instanceName ));
		}

		[Test]
		public void CanShutdownDatabaseManager() {
			var databaseFactory = MockRepository.GenerateStub<IDatabaseFactory>();
			var database = CreateDatabaseStub( "database" );

			databaseFactory.Stub( s => s.GetDatabaseInstance()).Return( database );

			var dbm = new DatabaseManager( databaseFactory );
			Assert.IsTrue( dbm.Initialize());

			var reserved = dbm.ReserveDatabase();
			dbm.FreeDatabase( reserved );

			dbm.Shutdown();
			database.AssertWasCalled( s => s.CloseDatabase());
			Assert.IsNull( dbm.ReserveDatabase());
		}

		private	bool	mDatabaseOpenCalled;

		[FixtureSetUp]
		public void FixtureSetup() {
			NoiseLogger.Current = MockRepository.GenerateStub<ILog>();
		}

		[SetUp]
		public void Setup() {
			mDatabaseOpenCalled = false;
		}

		private IDatabase CreateDatabaseStub( string instanceName ) {
			var retValue = MockRepository.GenerateStub<IDatabase>();

			retValue.Stub( s => s.DatabaseId ).Return( instanceName );
			retValue.Stub( s => s.InitializeDatabase()).Return( true );
			retValue.Stub( s => s.OpenWithCreateDatabase()).Return( true ).WhenCalled( method => mDatabaseOpenCalled = true );
			retValue.Stub( s => s.InitializeAndOpenDatabase()).Return( true ).WhenCalled( method => mDatabaseOpenCalled = true );

			return( retValue );
		}
	}
}
