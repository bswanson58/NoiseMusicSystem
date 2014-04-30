using FluentAssertions;
using Noise.EntityFrameworkDatabase.DatabaseUtility;
using NUnit.Framework;

namespace Noise.EntityFrameworkDatabase.Tests.DatabaseUtility {
	[TestFixture]
	public class SqlDatabaseManagerTests {

		private SqlDatabaseManager CreateSut() {
			return( new SqlDatabaseManager());
		}

		[Test]
		public void CanIterateDatabases() {
			var sut = CreateSut();

			var databases = sut.GetDatabaseList();
			databases.Should().NotBeEmpty( "There should be a list of databases" );
		}

		[Test]
		public void CanDetachDatabase() {
			var sut = CreateSut();

			sut.DetachDatabase( "INTEGRATION_TEST_DATABASE_01f85a2ae3a64187881f70cf71e824b2" );
		}
	}
}
