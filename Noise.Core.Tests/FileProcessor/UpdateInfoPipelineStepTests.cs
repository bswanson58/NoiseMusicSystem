using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.FileProcessor;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;

namespace Noise.Core.Tests.FileProcessor {
	[TestFixture]
	public class UpdateInfoPipelineStepTests {
		internal class TestableUpdateInfoPipelineStep : Testable<UpdateInfoPipelineStep> { }

		private DatabaseChangeSummary	mSummary;
		private StorageFile				mStorageFile;
		private DbArtist				mArtist;
		private DbAlbum					mAlbum;

		[SetUp]
		public void Setup() {
			mSummary = new DatabaseChangeSummary();
			mStorageFile = new StorageFile();
			mArtist = new DbArtist();
			mAlbum = new DbAlbum();

			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[Test]
		public void UpdateStepRequiresArtist() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary );

			sut.ProcessStep( context );
			testable.Mock<ITextInfoProvider>().Verify( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void UpdateStepRequiresAlbum() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary );

			sut.ProcessStep( context );
			testable.Mock<ITextInfoProvider>().Verify( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()), Times.Never());
		}

		[Test]
		public void UpdateStepAddsTextInfo() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			testable.Mock<ITextInfoProvider>().Verify( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void UpdateStepSetsArtist() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			DbTextInfo	info = null;

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<ITextInfoProvider>().Setup( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()))
																			.Callback<DbTextInfo, string>(( i, p ) => info = i  );

			sut.ProcessStep( context );

			info.Should().NotBeNull();
			info.Artist.Should().Be( mArtist.DbId );
		}


		[Test]
		public void UpdateStepSetsAlbum() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			DbTextInfo	info = null;

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<ITextInfoProvider>().Setup( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()))
																			.Callback<DbTextInfo, string>(( i, p ) => info = i  );

			sut.ProcessStep( context );

			info.Album.Should().Be( mAlbum.DbId );
		}

		[Test]
		public void UpdateStepSetsArtworkPath() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var storageFile = new StorageFile( "file name", 123, 321, DateTime.Now );
			var context = new PipelineContext( null, null, storageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			DbTextInfo	info = null;

			
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<ITextInfoProvider>().Setup( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()))
																			.Callback<DbTextInfo, string>(( i, p ) => info = i  );

			sut.ProcessStep( context );

			info.FolderLocation.Should().Be( storageFile.ParentFolder );
		}

		[Test]
		public void UpdateStepSetsArtworkName() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			const string fileName = "file name";
			var storageFile = new StorageFile( fileName + ".jpg", 123, 321, DateTime.Now );
			var context = new PipelineContext( null, null, storageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			DbTextInfo	info = null;

			
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<ITextInfoProvider>().Setup( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()))
																			.Callback<DbTextInfo, string>(( i, p ) => info = i  );

			sut.ProcessStep( context );

			info.Name.Should().Be( fileName );
		}

		[Test]
		public void UpdateShouldSetStorageFileMetaDataPointer() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();
			DbTextInfo	info = null;

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );
			testable.Mock<ITextInfoProvider>().Setup( m => m.AddTextInfo( It.IsAny<DbTextInfo>(), It.IsAny<string>()))
																			.Callback<DbTextInfo, string>(( i, p ) => info = i  );

			sut.ProcessStep( context );

			mStorageFile.MetaDataPointer.Should().Be( info.DbId );
		}

		[Test]
		public void UpdateShouldSetStorageFileTypeToMusic() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			mStorageFile.FileType.Should().Be( eFileType.Text );
		}

		[Test]
		public void UpdateShouldUpdateStorageFile() {
			var testable = new TestableUpdateInfoPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			updater.Verify( m => m.Update(), Times.Once());
		}
	}
}
