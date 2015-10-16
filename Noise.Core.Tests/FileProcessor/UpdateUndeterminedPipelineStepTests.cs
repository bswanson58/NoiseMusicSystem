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
	public class UpdateUndeterminedPipelineStepTests {
		internal class TestableUpdateUndeterminedPipelineStep : Testable<UpdateUndeterminedPipelineStep> { }

		private DatabaseChangeSummary		mSummary;
		private ILogLibraryClassification	mLog;

		[SetUp]
		public void Setup() {
			mSummary = new DatabaseChangeSummary();

			mLog = new Mock<ILogLibraryClassification>().Object;
		}

		[Test]
		public void UpdateShouldSetStorageFileTypeToUnknown() {
			var testable = new TestableUpdateUndeterminedPipelineStep();
			var sut = testable.ClassUnderTest;
			var storageFile = new StorageFile {FileType = eFileType.Undetermined };
			var context = new PipelineContext( null, null, storageFile, mSummary, mLog );
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( storageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( updater.Object );

			sut.ProcessStep( context );

			storageFile.FileType.Should().Be( eFileType.Unknown );
		}

		[Test]
		public void UpdateShouldUpdateStorageFile() {
			var testable = new TestableUpdateUndeterminedPipelineStep();
			var sut = testable.ClassUnderTest;
			var storageFile = new StorageFile {FileType = eFileType.Undetermined };
			var context = new PipelineContext( null, null, storageFile, mSummary, mLog );
			var updater = new Mock<IDataUpdateShell<StorageFile>>();

			updater.Setup( m => m.Item ).Returns( storageFile );
			testable.Mock<IStorageFileProvider>().Setup( m => m.GetFileForUpdate( storageFile.DbId )).Returns( updater.Object );

			sut.ProcessStep( context );

			updater.Verify( m => m.Update(), Times.Once());
		}
	}
}
