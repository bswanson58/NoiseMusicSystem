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

			var databaseName = sut.GetDatabaseName( "Local Noise.mdf" );
			sut.DetachDatabase( databaseName );
		}

		[Test]
		public void CanBackupDatabase() {
			var sut = CreateSut();

			var databaseName = sut.GetDatabaseName( "Local Noise.mdf" );
			sut.BackupDatabase( databaseName, @"D:\Local Noise.bak" );
		}

		[Test]
		public void CanGetDatabaseName() {
			var sut = CreateSut();

			var databaseName = sut.GetDatabaseName( "Local Noise.mdf" );
			databaseName.Should().NotBeNullOrEmpty( "A database name was not returned." );
		}

		[Test]
		public void CanGetRestoreFileList() {
			var sut = CreateSut();

			var fileList = sut.RestoreFileList( @"D:\Local Noise.bak" );
			fileList.Should().NotBeEmpty( "The backup should contain a file list." );
		}

		[Test]
		public void CanRestoreDatabase() {
			var sut = CreateSut();

			var databaseName = sut.GetDatabaseName( "Local Noise.mdf" );
			sut.DetachDatabase( databaseName );
			sut.RestoreFileList( @"D:\Local Noise.bak" );
			sut.RestoreDatabase( databaseName, @"D:\Local Noise.bak" );
		}
	}
}
