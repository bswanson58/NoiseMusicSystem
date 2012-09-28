﻿using System;
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
	public class FileTypePipelineStepTests {
		internal class TestableFileTypePipelineStep : Testable<FileTypePipelineStep> { }

		private StorageFile				mStorageFile;

		[SetUp]
		public void Setup() {
			mStorageFile = new StorageFile();

			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[Test]
		[ExpectedException( typeof( ArgumentNullException ))]
		public void PipelineStepRequireStorageFile() {
			var testable = new TestableFileTypePipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, null, null );

			sut.ProcessStep( context );
		}

		[Test]
		public void CanDetermineMusicFile() {
			var testable = new TestableFileTypePipelineStep();
			var context = new PipelineContext( null, null, mStorageFile, null );

			testable.Mock<IStorageFolderSupport>().Setup( m => m.DetermineFileType( mStorageFile )).Returns( eFileType.Music );

			var sut = testable.ClassUnderTest;
			sut.ProcessStep( context );

			context.Trigger.Should().Be( ePipelineTrigger.FileTypeIsAudio );
		}

		[Test]
		public void CanDetermineArtworkFile() {
			var testable = new TestableFileTypePipelineStep();
			var context = new PipelineContext( null, null, mStorageFile, null );

			testable.Mock<IStorageFolderSupport>().Setup( m => m.DetermineFileType( mStorageFile )).Returns( eFileType.Picture );

			var sut = testable.ClassUnderTest;
			sut.ProcessStep( context );

			context.Trigger.Should().Be( ePipelineTrigger.FileTypeIsArtwork );
		}

		[Test]
		public void CanDetermineInfoFile() {
			var testable = new TestableFileTypePipelineStep();
			var context = new PipelineContext( null, null, mStorageFile, null );

			testable.Mock<IStorageFolderSupport>().Setup( m => m.DetermineFileType( mStorageFile )).Returns( eFileType.Text );

			var sut = testable.ClassUnderTest;
			sut.ProcessStep( context );

			context.Trigger.Should().Be( ePipelineTrigger.FileTypeIsInfo );
		}

		[Test]
		public void MusicFileCreatesTrack() {
			var testable = new TestableFileTypePipelineStep();
			var context = new PipelineContext( null, null, mStorageFile, null );

			testable.Mock<IStorageFolderSupport>().Setup( m => m.DetermineFileType( mStorageFile )).Returns( eFileType.Music );

			var sut = testable.ClassUnderTest;
			sut.ProcessStep( context );

			context.Track.Should().NotBeNull();
		}
	}
}