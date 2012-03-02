using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseStorageFileProviderTests : BaseProviderTest<IStorageFileProvider> {

		[Test]
		public void CanAddStorageFile() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var sut = CreateSut();

			sut.AddFile( file );
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void CannotAddNullStorageFile() {
			var sut = CreateSut();

			sut.AddFile( null );
		}

		[Test]
		public void CanRetrieveStorageFile() {
			var file = new StorageFile( "filename", 1, 100, DateTime.Now );
			var sut = CreateSut();

			sut.AddFile( file );

			using( var fileList = sut.GetAllFiles()) {
				Assert.IsNotNull( fileList );
				Assert.IsNotNull( fileList.List );

				var retrieveFile = fileList.List.First();

				file.ShouldHave().AllPropertiesBut( p => p.FileModifiedDate ).EqualTo( retrieveFile );
			}
		}

		[Test]
		public void CanDeleteStorageFile() {
			var file = new StorageFile( "filename", 1, 100, DateTime.Now );
			var sut = CreateSut();

			sut.AddFile( file );
			sut.DeleteFile( file );

			using( var fileList = sut.GetAllFiles()) {
				fileList.List.Should().HaveCount( 0 );
			}
		}

		[Test]
		public void CanGetFileForTrack() {
			var track = new DbTrack();
			var file = new StorageFile( "filename", 1, 100, DateTime.Now ) { MetaDataPointer = track.DbId };
			var sut = CreateSut();

			sut.AddFile( file );

			var retrievedFile = sut.GetPhysicalFile( track );

			Assert.IsNotNull( retrievedFile );
		}

		[Test]
		public void CanRetrieveAllFiles() {
			var file1 = new StorageFile( "file one", 1, 100, DateTime.Now );
			var file2 = new StorageFile( "file two", 2, 100, DateTime.Now );
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			using( var fileList = sut.GetAllFiles()) {
				fileList.List.Should().HaveCount( 2 );
			}
		}

		[Test]
		public void CanRetrieveAllDeletedFiles() {
			var file1 = new StorageFile( "file one", 1, 100, DateTime.Now ) { IsDeleted = false };
			var file2 = new StorageFile( "file two", 2, 100, DateTime.Now ) { IsDeleted = true };
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			using( var fileList = sut.GetDeletedFilesList()) {
				fileList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanRetrieveAllFilesInFolder() {
			var parentFolder = new StorageFolder( "folder name", Constants.cDatabaseNullOid );
			var file1 = new StorageFile( "file one", parentFolder.DbId, 100, DateTime.Now );
			var file2 = new StorageFile( "file two", parentFolder.DbId + 1, 100, DateTime.Now );
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			using( var fileList = sut.GetFilesInFolder( parentFolder.DbId )) {
				fileList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFilesOfType() {
			var file1 = new StorageFile( "file one", 1, 100, DateTime.Now ) { FileType = eFileType.Text };
			var file2 = new StorageFile( "file two", 2, 100, DateTime.Now ) { FileType = eFileType.Undetermined };
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			using( var fileList = sut.GetFilesOfType( eFileType.Undetermined )) {
				fileList.List.Should().HaveCount( 1 );
			}
		}

		[Test]
		public void CanGetFileForUpdate() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now );
			var sut = CreateSut();
			sut.AddFile( file );

			using( var updater = sut.GetFileForUpdate( file.DbId )) {
				Assert.IsNotNull( updater );
				Assert.IsNotNull( updater.Item );

				file.ShouldHave().AllPropertiesBut( p => p.FileModifiedDate ).EqualTo( updater.Item );
			}
		}

		[Test]
		public void CanUpdateFile() {
			var file = new StorageFile( "file name", 1, 100, DateTime.Now ) { FileType = eFileType.Undetermined };
			var sut = CreateSut();
			sut.AddFile( file );

			using( var updater = sut.GetFileForUpdate( file.DbId )) {
				updater.Item.FileType = eFileType.Text;

				updater.Update();
			}

			using( var fileList = sut.GetAllFiles()) {
				var retrievedFile = fileList.List.First();

				retrievedFile.FileType.Should().Be( eFileType.Text );
			}
		}

		[Test]
		public void CanGetItemCount() {
			var file1 = new StorageFile( "file one", 1, 100, DateTime.Now );
			var file2 = new StorageFile( "file two", 2, 100, DateTime.Now );
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			var itemCount = sut.GetItemCount();

			itemCount.Should().Be( 2 );
		}
	}
}
