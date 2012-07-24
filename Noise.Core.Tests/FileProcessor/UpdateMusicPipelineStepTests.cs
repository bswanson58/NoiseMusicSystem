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
	public class UpdateMusicPipelineStepTests {
		internal class TestableUpdateMusicPipelineStep : Testable<UpdateMusicPipelineStep> { }

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
		[ExpectedException( typeof( ArgumentNullException ))]
		public void UpdateStepRequiresTrack() {
			var testable = new TestableUpdateMusicPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary );

			sut.ProcessStep( context );
		}

		[Test]
		public void UpdateStepRequiresArtist() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Track = track };

			sut.ProcessStep( context );
			testable.Mock<ITrackProvider>().Verify( m => m.AddTrack( track ), Times.Never());
		}

		[Test]
		public void UpdateStepRequiresAlbum() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Track = track };

			sut.ProcessStep( context );
			testable.Mock<ITrackProvider>().Verify( m => m.AddTrack( track ), Times.Never());
		}

		[Test]
		public void UpdateStepAddsTrack() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum, Track = track };

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			testable.Mock<ITrackProvider>().Verify( m => m.AddTrack( track ), Times.Once());
		}

		[Test]
		public void UpdateSetsTrackAlbumProperty() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum, Track = track };

			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			track.Album.Should().Be( mAlbum.DbId );
		}

		[Test]
		public void UpdateShouldSetStorageFileMetaDataPointer() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum, Track = track };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			mStorageFile.MetaDataPointer.Should().Be( track.DbId );
		}

		[Test]
		public void UpdateShouldSetStorageFileTypeToMusic() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum, Track = track };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			mStorageFile.FileType.Should().Be( eFileType.Music );
		}

		[Test]
		public void UpdateShouldUpdateStorageFile() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = mArtist, Album = mAlbum, Track = track };
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( mArtist.DbId )).Returns( new Mock<IDataUpdateShell<DbArtist>>().Object );

			sut.ProcessStep( context );

			updater.Verify( m => m.Update(), Times.Once());
		}

		[Test]
		public void UpdateShouldUpdateArtistAlbumCount() {
			var testable = new TestableUpdateMusicPipelineStep();
			var track = new DbTrack();
			var artist = new DbArtist { AlbumCount = 7 };
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary ) { Artist = artist, Album = mAlbum, Track = track };
			var updater = new Mock<IDataUpdateShell<DbArtist>>();

			updater.Setup( m => m.Item ).Returns( mArtist );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( new Mock<IDataUpdateShell<StorageFile>>().Object  );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistForUpdate( artist.DbId )).Returns( updater.Object );

			sut.ProcessStep( context );

			mArtist.AlbumCount.Should().Be( artist.AlbumCount );
		}
	}
}
