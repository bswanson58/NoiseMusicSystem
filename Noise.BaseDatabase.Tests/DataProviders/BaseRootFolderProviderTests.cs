using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseRootFolderProviderTests : BaseProviderTest<IRootFolderProvider> {
		protected IStorageFolderProvider	mStorageFolderProvider;

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
		public void RootFolderListReturnsFolderStrategies() {
			var folder = new RootFolder( 1, "path", "display name" );
			folder.FolderStrategy.SetStrategyForLevel( 0, eFolderStrategy.Artist );
			folder.FolderStrategy.SetStrategyForLevel( 1, eFolderStrategy.Album );
			folder.FolderStrategy.PreferFolderStrategy = true;

			var sut = CreateSut();
			sut.AddRootFolder( folder );

			using( var folderList = sut.GetRootFolderList()) {
				folderList.List.Should().HaveCount( 1 );

				var	retrievedFolder = folderList.List.First();
				retrievedFolder.ShouldHave().AllPropertiesBut( p => p.FolderStrategy ).EqualTo( folder );
				retrievedFolder.FolderStrategy.ShouldHave().AllProperties().EqualTo( folder.FolderStrategy );
			}
		}

		[Test]
		public void RootFolderListDoesNotReturnStorageFolders() {
			var rootFolder1 = new RootFolder( 1, "path 1", "folder 1" );
			var rootFolder2 = new RootFolder( 2, "path 2", "folder 2" );

			var sut = CreateSut();
			sut.AddRootFolder( rootFolder1 );
			sut.AddRootFolder( rootFolder2 );

			var storageFolder = new StorageFolder( "storage folder", 1 );
			mStorageFolderProvider.AddFolder( storageFolder );

			using( var folderList = sut.GetRootFolderList()) {
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

				folder.ShouldHave().AllPropertiesBut( p => p.FolderStrategy ).EqualTo( updater.Item );
				folder.FolderStrategy.ShouldHave().AllProperties().EqualTo( updater.Item.FolderStrategy );
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
