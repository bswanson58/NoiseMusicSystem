using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.IntegrationTests.Database {
	[TestFixture]
	public class StorageFileProviderTests : BaseDatabaseProviderTests {
		private Mock<IAlbumProvider>	mAlbumProvider;
		private ITrackProvider			mTrackProvider;
		private IStorageFolderProvider	mFolderProvider;

		[SetUp]
		public override void Setup() {
			base.Setup();

			mAlbumProvider = new Mock<IAlbumProvider>();
			mTrackProvider = new TrackProvider( mDatabaseManager );
			mFolderProvider = new StorageFolderProvider( mDatabaseManager );
		}

		private IStorageFileProvider CreateSut() {
			return( new StorageFileProvider( mDatabaseManager, mAlbumProvider.Object, mTrackProvider, mFolderProvider ));
		}

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

				file.ShouldHave().AllProperties().EqualTo( retrieveFile );
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
		public void CanGetPathForStorageFile() {
			var folder1 = new StorageFolder( "one", Constants.cDatabaseNullOid );
			var folder2 = new StorageFolder( "two", folder1.DbId );

			mFolderProvider.AddFolder( folder1 );
			mFolderProvider.AddFolder( folder2 );

			var file = new StorageFile( "file name", folder2.DbId, 100, DateTime.Now );
			var sut = CreateSut();
			var path = sut.GetPhysicalFilePath( file );
			
			path.Should().Be( Path.Combine( folder1.Name, Path.Combine( folder2.Name, file.Name )));
		}

		[Test]
		public void CanGetAlbumPathForStorageFile() {
			var parentFolder = new StorageFolder( "parent", Constants.cDatabaseNullOid );
			var folder1 = new StorageFolder( "volume 1", parentFolder.DbId );
			var folder2 = new StorageFolder( "volume 2", parentFolder.DbId );

			mFolderProvider.AddFolder( parentFolder );
			mFolderProvider.AddFolder( folder1 );
			mFolderProvider.AddFolder( folder2 );

			var album = new DbAlbum();
			mAlbumProvider.Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var	track1 = new DbTrack { Album = album.DbId };
			var track2 = new DbTrack { Album = album.DbId };

			mTrackProvider.AddTrack( track1 );
			mTrackProvider.AddTrack( track2 );

			var file1 = new StorageFile( "file one", folder1.DbId, 100, DateTime.Now ) { MetaDataPointer = track1.DbId };
			var file2 = new StorageFile( "file two", folder2.DbId, 100, DateTime.Now ) { MetaDataPointer = track2.DbId };
			var sut = CreateSut();
			sut.AddFile( file1 );
			sut.AddFile( file2 );

			var path = sut.GetAlbumPath( album.DbId );
			
			path.Should().Be( parentFolder.Name + @"\" );
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

				file.ShouldHave().AllProperties().EqualTo( updater.Item );
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
