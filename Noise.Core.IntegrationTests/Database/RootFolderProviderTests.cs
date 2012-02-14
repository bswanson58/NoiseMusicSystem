using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Core.FileStore;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class RootFolderProviderTests : BaseDatabaseProviderTests {

		private IRootFolderProvider CreateSut() {
			return( new RootFolderProvider( mDatabaseManager ));
		}

		[Test]
		public void CanAddRootFolder() {
			var folder = new RootFolder( 1, "path", "folder name" );
			var sut = CreateSut();

			sut.AddRootFolder( folder );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullRootFolder() {
			var sut = CreateSut();

			sut.AddRootFolder( null );
		}

		[Test]
		public void CannotAddDuplicateFolder() {
			var folder = new RootFolder( 1, "path", "folder name" );
			var sut = CreateSut();

			sut.AddRootFolder( folder );
			sut.AddRootFolder( folder );

			using( var folderList = sut.GetRootFolderList()) {
				folderList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetRootFolderList() {
			var folder1 = new RootFolder( 1, "path 1", "folder 1" );
			var folder2 = new RootFolder( 2, "path 2", "folder 2" );

			var sut = CreateSut();
			sut.AddRootFolder( folder1 );
			sut.AddRootFolder( folder2 );

			using( var folderList = sut.GetRootFolderList()) {
				Assert.IsNotNull( folderList );
				Assert.IsNotNull( folderList.List );

				folderList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetFolderForUpdate() {
			var folder = new RootFolder( 1, "path", "folder name" );
			var sut = CreateSut();

			sut.AddRootFolder( folder );

			using( var updater = sut.GetFolderForUpdate( folder.DbId )) {
				Assert.IsNotNull( updater );
				Assert.IsNotNull( updater.Item );

				folder.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateFolder() {
			var folder = new RootFolder( 1, "path", "original name" );
			var sut = CreateSut();

			sut.AddRootFolder( folder );

			using( var updater = sut.GetFolderForUpdate( folder.DbId )) {
				updater.Item.Name = "changed name";

				updater.Update();
			}

			using( var folderList = sut.GetRootFolderList()) {
				var retrievedFolder = folderList.List.First();

				folder.Name.Should().NotBe( retrievedFolder.Name );
			}
		}

	}
}
