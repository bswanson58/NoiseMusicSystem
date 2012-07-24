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
	public class UpdateUndeterminedPipelineStepTests {
		internal class TestableUpdateUndeterminedPipelineStep : Testable<UpdateUndeterminedPipelineStep> { }

		private DatabaseChangeSummary	mSummary;
		private StorageFile				mStorageFile;

		[SetUp]
		public void Setup() {
			mSummary = new DatabaseChangeSummary();
			mStorageFile = new StorageFile();

			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[Test]
		public void UpdateShouldSetStorageFileTypeToUndetermined() {
			var testable = new TestableUpdateUndeterminedPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary );
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );

			sut.ProcessStep( context );

			mStorageFile.FileType.Should().Be( eFileType.Undetermined );
		}

		[Test]
		public void UpdateShouldUpdateStorageFile() {
			var testable = new TestableUpdateUndeterminedPipelineStep();
			var sut = testable.ClassUnderTest;
			var context = new PipelineContext( null, null, mStorageFile, mSummary );
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( mStorageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( mStorageFile.DbId )).Returns( updater.Object );

			sut.ProcessStep( context );

			updater.Verify( m => m.Update(), Times.Once());
		}
	}
}
