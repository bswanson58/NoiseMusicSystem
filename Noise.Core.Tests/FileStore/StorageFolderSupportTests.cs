using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;

namespace Noise.Core.Tests.FileStore {
	[TestFixture]
	public class StorageFolderSupportTests {
		internal class TestableStorageFolderSupport : Testable<StorageFolderSupport> {
			
		}

		[Test]
		public void CanGetPathForFolder() {
			var testable = new TestableStorageFolderSupport();

			var parentFolder = new RootFolder( 1, "parent", "display name" );
			var folder1 = new StorageFolder( "volume 1", parentFolder.DbId );
			var folder2 = new StorageFolder( "volume 2", folder1.DbId );

			testable.Mock<IRootFolderProvider>().Setup( m => m.GetRootFolder( It.Is<long>( p => p == parentFolder.DbId ))).Returns( parentFolder );
			testable.Mock<IStorageFolderProvider>().Setup( m => m.GetFolder( folder1.DbId )).Returns( folder1 );
			testable.Mock<IStorageFolderProvider>().Setup( m => m.GetFolder( folder2.DbId )).Returns( folder2 );

			var sut = testable.ClassUnderTest;
			var path = sut.GetPath( folder2 );

			var expectedPath = Path.Combine( parentFolder.Name, Path.Combine( folder1.Name, folder2.Name ));
			path.Should().Be( expectedPath );
		}

		[Test]
		public void CanGetPathForFile() {
			var testable = new TestableStorageFolderSupport();

			var parentFolder = new RootFolder( 1, "parent", "display name" );
			var folder = new StorageFolder( "volume 1", parentFolder.DbId );
			var file = new StorageFile( "file Name", folder.DbId, 100, DateTime.Now );

			testable.Mock<IRootFolderProvider>().Setup( m => m.GetRootFolder( It.Is<long>( p => p == parentFolder.DbId ))).Returns( parentFolder );
			testable.Mock<IStorageFolderProvider>().Setup( m => m.GetFolder( folder.DbId )).Returns( folder );

			var sut = testable.ClassUnderTest;
			var path = sut.GetPath( file );

			var expectedPath = Path.Combine( parentFolder.Name, Path.Combine( folder.Name, file.Name ));
			path.Should().Be( expectedPath );
		}

		[Test]
		public void CanGetAlbumPathForStorageFile() {
			var testable = new TestableStorageFolderSupport();

			var parentFolder = new RootFolder( 1, "parent", "display name" );
			var folder1 = new StorageFolder( "volume 1", parentFolder.DbId );
			var folder2 = new StorageFolder( "volume 2", parentFolder.DbId );

			testable.Mock<IRootFolderProvider>().Setup( m => m.GetRootFolder( It.Is<long>( p => p == parentFolder.DbId ))).Returns( parentFolder );
			testable.Mock<IStorageFolderProvider>().Setup( m => m.GetFolder( folder1.DbId )).Returns( folder1 );
			testable.Mock<IStorageFolderProvider>().Setup( m => m.GetFolder( folder2.DbId )).Returns( folder2 );

			var album = new DbAlbum();
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( It.IsAny<long>())).Returns( album );

			var	track1 = new DbTrack { Album = album.DbId };
			var track2 = new DbTrack { Album = album.DbId };
			var trackList = new List<DbTrack> { track1, track2 };
			var dataProvider = new Mock<IDataProviderList<DbTrack>>();
 			dataProvider.Setup( m => m.List ).Returns( trackList );
			testable.Mock<ITrackProvider>().Setup( m => m.GetTrackList( album.DbId )).Returns( dataProvider.Object );

			var file1 = new StorageFile( "file one", folder1.DbId, 100, DateTime.Now ) { MetaDataPointer = track1.DbId };
			var file2 = new StorageFile( "file two", folder2.DbId, 100, DateTime.Now ) { MetaDataPointer = track2.DbId };
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetPhysicalFile( track1 )).Returns( file1 );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetPhysicalFile( track2 )).Returns( file2 );

			var sut = testable.ClassUnderTest;
			var path = sut.GetAlbumPath( album.DbId );
			
			path.Should().Be( parentFolder.Name + @"\" );
		}
	}
}
