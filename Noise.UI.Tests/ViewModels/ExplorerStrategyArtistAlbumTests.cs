using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.ViewModels;
using ReusableBits.TestSupport.Mocking;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableStrategyArtistAlbum : Testable<ExplorerStrategyArtistAlbum> { }

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
	}
}
