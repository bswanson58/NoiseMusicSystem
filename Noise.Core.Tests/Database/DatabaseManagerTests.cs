using MbUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure.Interfaces;
using Rhino.Mocks;

namespace Noise.Core.Tests.Database {
	public class DatabaseManagerTests {
		private readonly MockRepository	mMockRepository;

		public DatabaseManagerTests() {
			mMockRepository = new MockRepository();
		}

		[Test]
		public void CanCreateDatabase() {
			var databaseFactory = mMockRepository.StrictMock<IDatabaseFactory>();
			var database = mMockRepository.StrictMock<IDatabase>();

			Expect.Call( databaseFactory.GetDatabaseInstance()).Return( database );
			Expect.Call( database.InitializeDatabase()).Return( true );
			Expect.Call( database.OpenWithCreateDatabase()).Return( true );

			var dbm = new DatabaseManager( databaseFactory );
			//Assert.IsTrue( dbm.Initialize());
		}
	}
}
