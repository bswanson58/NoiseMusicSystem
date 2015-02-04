using System;
using System.Collections.Generic;
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
	public class StorageFileProcessorTests {
		public class TestPipelineStep : BasePipelineStep {
			public TestPipelineStep( ePipelineStep step ) :
				base( step ) { }

			public override void ProcessStep( PipelineContext context ) {
			}
		}

		internal class TestableStorageFileProcessor : Testable<StorageFileProcessor> { }

		private List<DbArtist>			mArtistCacheList;
		private List<DbAlbum>			mAlbumCacheList;
		private List<StorageFile>		mFileList;
		private List<IPipelineStep>		mPipelineSteps; 
		private DatabaseChangeSummary	mSummary;

		private Mock<TestPipelineStep>		mArtworkProviders;
		private Mock<TestPipelineStep>		mCompleted;
		private Mock<TestPipelineStep>		mDetermineAlbum;
		private Mock<TestPipelineStep>		mDetermineArtist;
		private Mock<TestPipelineStep>		mDetermineTrack;
		private Mock<TestPipelineStep>		mDetermineVolume;
		private Mock<TestPipelineStep>		mFileType;
		private Mock<TestPipelineStep>		mInfoProviders;
		private Mock<TestPipelineStep>		mMusicMetadata;
		private Mock<TestPipelineStep>		mMusicProviders;
		private Mock<TestPipelineStep>		mUpdateArtwork;
		private Mock<TestPipelineStep>		mUpdateInfo;
		private Mock<TestPipelineStep>		mUpdateMusic;
		private Mock<TestPipelineStep>		mUpdateUndetermined;

		[SetUp]
		public void Setup() {
			mArtistCacheList = new List<DbArtist>();
			mAlbumCacheList = new List<DbAlbum>();
			mFileList = new List<StorageFile>();
			mSummary = new DatabaseChangeSummary();

			mArtworkProviders = new Mock<TestPipelineStep>( ePipelineStep.BuildArtworkProviders );
			mCompleted = new Mock<TestPipelineStep>( ePipelineStep.Completed );
			mDetermineAlbum = new Mock<TestPipelineStep>( ePipelineStep.DetermineAlbum );
			mDetermineArtist = new Mock<TestPipelineStep>( ePipelineStep.DetermineArtist );
			mDetermineTrack = new Mock<TestPipelineStep>( ePipelineStep.DetermineTrackName );
			mDetermineVolume = new Mock<TestPipelineStep>( ePipelineStep.DetermineVolume );
			mFileType = new Mock<TestPipelineStep>( ePipelineStep.DetermineFileType );
			mInfoProviders = new Mock<TestPipelineStep>( ePipelineStep.BuildInfoProviders );
			mMusicMetadata = new Mock<TestPipelineStep>( ePipelineStep.AddMusicMetadata );
			mMusicProviders = new Mock<TestPipelineStep>( ePipelineStep.BuildMusicProviders );
			mUpdateArtwork = new Mock<TestPipelineStep>( ePipelineStep.UpdateArtwork );
			mUpdateInfo = new Mock<TestPipelineStep>( ePipelineStep.UpdateInfo );
			mUpdateMusic = new Mock<TestPipelineStep>( ePipelineStep.UpdateMusic );
			mUpdateUndetermined = new Mock<TestPipelineStep>( ePipelineStep.UpdateUndetermined );

			mCompleted.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.Completed );

			mPipelineSteps = new List<IPipelineStep> { mArtworkProviders.Object, mCompleted.Object, mDetermineAlbum.Object,
													   mDetermineArtist.Object, mDetermineTrack.Object,
													   mDetermineVolume.Object, mFileType.Object, mInfoProviders.Object,
													   mMusicMetadata.Object, mMusicProviders.Object, mUpdateArtwork.Object,
													   mUpdateInfo.Object, mUpdateMusic.Object, mUpdateUndetermined.Object };

			NoiseLogger.Current = new Mock<INoiseLog>().Object;
		}

		private TestableStorageFileProcessor CreateSut() {
			var retValue = new TestableStorageFileProcessor();

			retValue.InjectArray( mPipelineSteps.ToArray());

			var artistProvider = new Mock<IDataProviderList<DbArtist>>(); 
			artistProvider.Setup( m => m.List ).Returns( mArtistCacheList );
			retValue.Mock<IArtistProvider>().Setup( m => m.GetArtistList()).Returns( artistProvider.Object );

			var albumProvider = new Mock<IDataProviderList<DbAlbum>>(); 
			albumProvider.Setup( m => m.List ).Returns( mAlbumCacheList );
			retValue.Mock<IAlbumProvider>().Setup( m => m.GetAllAlbums()).Returns( albumProvider.Object );

			var fileProvider = new Mock<IDataProviderList<StorageFile>>(); 
			fileProvider.Setup( m => m.List ).Returns( mFileList );
			retValue.Mock<IStorageFileProvider>().Setup( m => m.GetFilesRequiringProcessing()).Returns( fileProvider.Object );

			return( retValue );
		}

		[Test]
		public void CanCreateProcessor() {
			var sut = new TestableStorageFileProcessor();

			sut.Should().NotBeNull();
		}

		[Test]
		public void CanProcessEmptyFileList() {
			var sut = CreateSut();
			var summary = new Mock<DatabaseChangeSummary>();

			sut.ClassUnderTest.Process( summary.Object );
		}

		[Test]
		public void CanProcessMusicFile() {
			mFileList.Add( new StorageFile( "music.mp3", 0, 100, DateTime.Now ));

			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsAudio );
			var sut = CreateSut();

			sut.ClassUnderTest.Process( mSummary );
			mUpdateMusic.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Once());
		}

		[Test]
		public void CanProcessMusicUpdate() {
			mFileList.Add( new StorageFile( "update.mp3", 0, 100, DateTime.Now ) { WasUpdated = true });

			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>() ) ).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsAudio );
			var sut = CreateSut();

			sut.ClassUnderTest.Process( mSummary );
			mUpdateMusic.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>() ), Times.Once() );
		}

		[Test]
		public void CanProcessArtworkFile() {
			mFileList.Add( new StorageFile( "cover.jpg", 0, 100, DateTime.Now ));

			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsArtwork );
			var sut = CreateSut();

			sut.ClassUnderTest.Process( mSummary );
			mUpdateArtwork.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Once());
		}

		[Test]
		public void CanProcessTextFile() {
			mFileList.Add( new StorageFile( "info.txt", 0, 100, DateTime.Now ));

			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsInfo );
			var sut = CreateSut();

			sut.ClassUnderTest.Process( mSummary );
			mUpdateInfo.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Once());
		}

		[Test]
		public void CanProcessUnusedFile() {
			mFileList.Add( new StorageFile( "something.xyz", 0, 100, DateTime.Now ));

			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsUnknown );
			var sut = CreateSut();

			sut.ClassUnderTest.Process( mSummary );
			mUpdateUndetermined.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Once());
		}

		[Test]
		public void CanProcessMultipleMusicFiles() {
			mFileList.Add( new StorageFile( "tune1.mp3", 0, 100, DateTime.Now ));
			mFileList.Add( new StorageFile( "tune2.mp3", 0, 100, DateTime.Now ));
			mFileList.Add( new StorageFile( "tune3.mp3", 0, 100, DateTime.Now ));

			var sut = CreateSut();
			
			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsAudio );

			sut.ClassUnderTest.Process( mSummary );
			mUpdateMusic.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Exactly( 3 ));
		}

		[Test]
		public void CanStopProcessing() {
			mFileList.Add( new StorageFile( "tune1.mp3", 0, 100, DateTime.Now ));
			mFileList.Add( new StorageFile( "tune2.mp3", 0, 100, DateTime.Now ));

			var sut = CreateSut();
			
			mFileType.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback<PipelineContext>( context => context.Trigger = ePipelineTrigger.FileTypeIsAudio );
			mUpdateMusic.Setup( m => m.ProcessStep( It.IsAny<PipelineContext>())).Callback( () => sut.ClassUnderTest.Stop());

			sut.ClassUnderTest.Process( mSummary );
			mUpdateMusic.Verify( m => m.ProcessStep( It.IsAny<PipelineContext>()), Times.Once());
		}
	}
}
