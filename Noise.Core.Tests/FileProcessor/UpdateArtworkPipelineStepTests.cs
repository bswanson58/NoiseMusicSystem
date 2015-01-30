using System;
using FluentAssertions;
using Moq;
using Noise.Core.Logging;
using NUnit.Framework;
using Noise.Core.FileProcessor;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;

namespace Noise.Core.Tests.FileProcessor {
	[TestFixture]
	public class UpdateArtworkPipelineStepTests {
		internal class TestableUpdateArtworkPipelineStep : Testable<UpdateArtworkPipelineStep> { }

		private ILogLibraryClassification	mLog;
		private DatabaseChangeSummary		mSummary;
		private StorageFile					mStorageFile;
		private DbArtist					mArtist;
		private DbAlbum						mAlbum;

		[SetUp]
		public void Setup() {
			mSummary = new DatabaseChangeSummary();
			mStorageFile = new StorageFile();
			mArtist = new DbArtist();
			mAlbum = new DbAlbum();

			mLog = new Mock<ILogLibraryClassification>().Object;
		}

		[Test]
		public void UpdateStepRequiresArtist() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog );

			sut.ProcessStep( context );
			testable.Mock<IArtworkProvider>().Verify( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void UpdateStepRequiresAlbum() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog );

			sut.ProcessStep( context );
			testable.Mock<IArtworkProvider>().Verify( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void UpdateStepAddsArtwork() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			testable.Mock<IArtworkProvider>().Verify( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void UpdateStepSetsArtist() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			DbArtwork	artwork = null;

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<IArtworkProvider>().Setup( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()))
																			.Callback<DbArtwork, string>(( a, p ) => artwork = a  );

			sut.ProcessStep( context );

			artwork.Should().NotBeNull();
			artwork.Artist.Should().Be( mArtist.DbId );
		}


		[Test]
		public void UpdateStepSetsAlbum() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			DbArtwork	artwork = null;

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<IArtworkProvider>().Setup( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()))
																			.Callback<DbArtwork, string>(( a, p ) => artwork = a  );

			sut.ProcessStep( context );

			artwork.Should().NotBeNull();
			artwork.Album.Should().Be( mAlbum.DbId );
		}

		[Test]
		public void UpdateStepSetsArtworkPath() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var storageFile = new StorageFile( "file name", 123, 321, DateTime.Now );
			var context = new PipelineContext( null, null, storageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			DbArtwork	artwork = null;

			
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<IArtworkProvider>().Setup( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()))
																			.Callback<DbArtwork, string>(( a, p ) => artwork = a  );

			sut.ProcessStep( context );

			artwork.Should().NotBeNull();
			artwork.FolderLocation.Should().Be( storageFile.ParentFolder );
		}

		[Test]
		public void UpdateStepSetsArtworkName() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			const string fileName = "file name";
			var storageFile = new StorageFile( fileName + ".jpg", 123, 321, DateTime.Now );
			var context = new PipelineContext( null, null, storageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			DbArtwork	artwork = null;

			
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<IArtworkProvider>().Setup( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()))
																			.Callback<DbArtwork, string>(( a, p ) => artwork = a  );

			sut.ProcessStep( context );

			artwork.Should().NotBeNull();
			artwork.Name.Should().Be( fileName );
		}

		[Test]
		public void UpdateShouldSetStorageFileMetaDataPointer() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();
			DbArtwork	artwork = null;

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<IArtworkProvider>().Setup( m => m.AddArtwork( It.IsAny<DbArtwork>(), It.IsAny<string>()))
																			.Callback<DbArtwork, string>(( a, p ) => artwork = a  );

			sut.ProcessStep( context );

			mStorageFile.MetaDataPointer.Should().Be( artwork.DbId );
		}

		[Test]
		public void UpdateShouldSetStorageFileTypeToMusic() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			mStorageFile.FileType.Should().Be( eFileType.Picture );
		}

		[Test]
		public void UpdateShouldUpdateStorageFile() {
			var testable = new TestableUpdateArtworkPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary, mLog ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			updater.Verify( m => m.Update(), Times.Once());
		}
	}
}
