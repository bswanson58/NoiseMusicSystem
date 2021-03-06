﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseStorageFolderProviderTests : BaseProviderTest<IStorageFolderProvider> {
		[Test]
		public void CanAddStorageFolder() {
			var sut = CreateSut();
			var folder = new StorageFolder( "folder name", 1 );

			sut.AddFolder( folder );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullFolder() {
			var sut = CreateSut();

			sut.AddFolder( null );
		}

		[Test]
		public void CanStoreAllFolderProperties() {
			var folder = new StorageFolder( "my folder", 1 ) { IsDeleted = true };

			var sut = CreateSut();
			sut.AddFolder( folder );

			var result = sut.GetFolder( folder.DbId );
			result.ShouldHave().AllProperties().EqualTo( folder );
		}

		[Test]
		public void CanRetrieveFolder() {
			var folder = new StorageFolder( "folder name", 1 );
			var sut = CreateSut();
			sut.AddFolder( folder );

			var retrievedFolder = sut.GetFolder( folder.DbId );

			folder.ShouldHave().AllProperties().EqualTo( retrievedFolder );
		}

		[Test]
		public void CanRemoveFolder() {
			var folder = new StorageFolder( "folder name", 1 );
			var sut = CreateSut();
			sut.AddFolder( folder );
			sut.RemoveFolder( folder );

			var retrievedFolder = sut.GetFolder( folder.DbId );

			Assert.IsNull( retrievedFolder );
		}

		[Test]
		public void CannotAddDuplicateFolder() {
			var folder = new StorageFolder( "folder name", 1 );
			var sut = CreateSut();

			sut.AddFolder( folder );
			sut.AddFolder( folder );

			using( var folderList = sut.GetAllFolders()) {
				folderList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFolderList() {
			var folder1 = new StorageFolder( "one", 1 );
			var folder2 = new StorageFolder( "two", 2 );

			var sut = CreateSut();
			sut.AddFolder( folder1 );
			sut.AddFolder( folder2 );

			using( var folderList = sut.GetAllFolders()) {
				Assert.IsNotNull( folderList );
				Assert.IsNotNull( folderList.List );

				folderList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetChildFolders() {
			var parentFolder = new StorageFolder( "parent", Constants.cDatabaseNullOid );
			var child1 = new StorageFolder( "child 1", parentFolder.DbId );
			var child2 = new StorageFolder( "child 2", parentFolder.DbId );

			var sut = CreateSut();
			sut.AddFolder( child1 );
			sut.AddFolder( child2 );

			using( var folderList = sut.GetChildFolders( parentFolder.DbId )) {
				Assert.IsNotNull( folderList );
				Assert.IsNotNull( folderList.List );

				folderList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetDeletedFolderList() {
			var folder1 = new StorageFolder( "folder 1", 1 ) { IsDeleted = true };
			var folder2 = new StorageFolder( "folder 2", 1 ) { IsDeleted = false };
			var folder3 = new StorageFolder( "folder 3", 1 ) { IsDeleted = true };

			var sut = CreateSut();
			sut.AddFolder( folder1 );
			sut.AddFolder( folder2 );
			sut.AddFolder( folder3 );

			using( var folderList = sut.GetDeletedFolderList()) {
				Assert.IsNotNull( folderList );
				Assert.IsNotNull( folderList.List );

				folderList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanGetFolderForUpdate() {
			var folder = new StorageFolder( "folder name", 1 );
			var sut = CreateSut();

			sut.AddFolder( folder );

			using( var updater = sut.GetFolderForUpdate( folder.DbId )) {
				Assert.IsNotNull( updater );
				Assert.IsNotNull( updater.Item );

				folder.ShouldHave().AllProperties().EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateFolder() {
			var folder = new StorageFolder( "original name", 1 );
			var sut = CreateSut();

			sut.AddFolder( folder );

			using( var updater = sut.GetFolderForUpdate( folder.DbId )) {
				updater.Item.Name = "changed name";

				updater.Update();
			}

			var retrievedFolder = sut.GetFolder( folder.DbId );

			folder.Name.Should().NotBe( retrievedFolder.Name );
		}

		[Test]
		public void CanGetItemCount() {
			var folder1 = new StorageFolder( "one", 1 );
			var folder2 = new StorageFolder( "two", 2 );

			var sut = CreateSut();
			sut.AddFolder( folder1 );
			sut.AddFolder( folder2 );

			var folderCount = sut.GetItemCount();

			folderCount.Should().Be( 2 );
		}
	}
}
