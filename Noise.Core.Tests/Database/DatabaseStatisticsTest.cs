using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Tests.Database {
	[TestFixture]
	public class DatabaseStatisticsTest {
		private Mock<IArtistProvider>			mArtistProvider;
		private Mock<IAlbumProvider>			mAlbumProvider;
		private Mock<ITrackProvider>			mTrackProvider;
		private Mock<IRootFolderProvider>		mRootFolderProvider;
		private Mock<IStorageFolderProvider>	mStorageFolderProvider;
		private Mock<IStorageFileProvider>		mStorageFileProvider;

		[SetUp]
		private void Setup() {
			mArtistProvider = new Mock<IArtistProvider>();
			mAlbumProvider = new Mock<IAlbumProvider>();
			mTrackProvider = new Mock<ITrackProvider>();
			mRootFolderProvider = new Mock<IRootFolderProvider>();
			mStorageFolderProvider = new Mock<IStorageFolderProvider>();
			mStorageFileProvider = new Mock<IStorageFileProvider>();
		}

		private DatabaseStatistics CreateSut() {
			return( new DatabaseStatistics( mArtistProvider.Object, mAlbumProvider.Object, mTrackProvider.Object,
											mRootFolderProvider.Object, mStorageFolderProvider.Object, mStorageFileProvider.Object ));
		}

		[Test]
		public void CanCountArtists() {
			mArtistProvider.Setup( m => m.GetItemCount()).Returns( 3 );

			var sut = CreateSut();
			sut.GatherStatistics( false );

			var artistCount = sut.ArtistCount;
			artistCount.Should().Be( 3 );
		}

		[Test]
		public void CanCountAlbums() {
			mAlbumProvider.Setup( m => m.GetItemCount()).Returns( 4 );

			var sut = CreateSut();
			sut.GatherStatistics( false );

			var albumCount = sut.AlbumCount;

			albumCount.Should().Be( 4 );
		}

		[Test]
		public void CanCountTracks() {
			mTrackProvider.Setup( m => m.GetItemCount()).Returns( 5 );

			var sut = CreateSut();
			sut.GatherStatistics( true );

			var trackCount = sut.TrackCount;

			trackCount.Should().Be( 5 );
		}

		[Test]
		public void CanCountFolders() {
			mStorageFolderProvider.Setup( m => m.GetItemCount()).Returns( 6 );

			var sut = CreateSut();
			sut.GatherStatistics( true );

			var folderCount = sut.FolderCount;

			folderCount.Should().Be( 6 );
		}

		[Test]
		public void CanCountFiles() {
			mStorageFileProvider.Setup( m => m.GetItemCount()).Returns( 7 );

			var sut = CreateSut();
			sut.GatherStatistics( true );

			var fileCount = sut.FileCount;

			fileCount.Should().Be( 7 );
		}

		[Test]
		public void CanGetLastScanTime() {
			var databaseShell = new Mock<IDatabaseShell>();
			var rootFolder = new RootFolder( 1, "path", "name" );
			rootFolder.UpdateLibraryScan();

			var list = new DataProviderList<RootFolder>( databaseShell.Object, new List<RootFolder>{ rootFolder });

			mRootFolderProvider.Setup( m => m.GetRootFolderList()).Returns( list );

			var sut = CreateSut();
			sut.GatherStatistics( true );

			var lastScan = sut.LastScan;

			rootFolder.LastLibraryScan.Should().Be( lastScan.Ticks );
		}

		[Test]
		public void CanProviderSummaryString() {
			var sut = CreateSut();

			sut.GatherStatistics( false );

			var summary = sut.ToString();
			Assert.IsNotNullOrEmpty( summary );
		}
	}
}
