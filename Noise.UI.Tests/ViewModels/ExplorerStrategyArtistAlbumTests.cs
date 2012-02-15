using System.Windows;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits.TestSupport.Mocking;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableStrategyArtistAlbum : Testable<ExplorerStrategyArtistAlbum> {
		private readonly DataTemplate	mDataTemplate;

		public TestableStrategyArtistAlbum() {
			mDataTemplate = new DataTemplate();

			Mock<IResourceProvider>().Setup( m => m.RetrieveTemplate( It.IsAny<string>())).Returns( mDataTemplate );
		}
	}

	[TestFixture]
	public class ExplorerStrategyArtistAlbumTests {
		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		[Test]
		public void CanCreateStrategy() {
			var testable = new TestableStrategyArtistAlbum();

			var sut = testable.ClassUnderTest;

			Assert.IsNotNullOrEmpty( sut.StrategyId );
			Assert.IsNotNullOrEmpty( sut.StrategyName );
		}

		[Test]
		public void CanInitializeStrategy() {
			var	testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();
			var sut = testable.ClassUnderTest;

			sut.Initialize( viewModel.Object );
		}

		[Test]
		public void ActivateSetsViewTemplate() {
			var testable = new TestableStrategyArtistAlbum();
			var viewModel = new Mock<ILibraryExplorerViewModel>();

			viewModel.Setup( m => m.SetViewTemplate( It.IsAny<DataTemplate>())).Verifiable();

			var sut = testable.ClassUnderTest;
			sut.Initialize( viewModel.Object );

			sut.Activate();

			viewModel.Verify();
		}
	}
}
