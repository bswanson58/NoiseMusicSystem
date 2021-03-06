﻿using FluentAssertions;
using Noise.EntityFrameworkDatabase.DatabaseUtility;
using Noise.Infrastructure.Interfaces;
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
		public void CanAttachDatabase() {
			var sut = CreateSut();

			sut.AttachDatabase( "Local Noise.mdf", @"" );
		}

		[Test]
		public void CanDetachDatabase() {
			var sut = CreateSut();

			var databaseName = sut.GetDatabaseName( "Local Noise.mdf" );
			sut.DetachDatabase( new DatabaseInfo( databaseName ));
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
			if( !string.IsNullOrWhiteSpace( databaseName )) {
				sut.DetachDatabase( new DatabaseInfo( databaseName ));
			}
			else {
				databaseName = "Local Noise.mdf";
			}

			sut.RestoreFileList( @"D:\Local Noise.bak" );
			sut.RestoreDatabase( "foo", @"D:\Local Noise.bak" );
		}
	}
}
