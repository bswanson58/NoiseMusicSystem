using Moq;
using NUnit.Framework;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.Tests.MockingEventAggregator;
using Noise.UI.ViewModels;

namespace Noise.UI.Tests.ViewModels {
	[TestFixture]
	public class ArtistViewModelTests {
		private AutoMockingEventAggregator	mEvents;
		private Mock<IArtistProvider>		mArtistProvider;
		private Mock<IAlbumProvider>		mAlbumProvider;
		private Mock<IDiscographyProvider>	mDiscographyProvider;
		private Mock<ITagManager>			mTagManager;
		private Mock<IDialogService>		mDialogService;

		[SetUp]
		public void Setup() {
			mEvents = new AutoMockingEventAggregator();
			mArtistProvider = new Mock<IArtistProvider>();
			mAlbumProvider = new Mock<IAlbumProvider>();
			mDiscographyProvider = new Mock<IDiscographyProvider>();
			mTagManager = new Mock<ITagManager>();
			mDialogService = new Mock<IDialogService>();
		}
		private ArtistViewModel CreateSut() {
			return( new ArtistViewModel( mEvents, mArtistProvider.Object, mAlbumProvider.Object, mDiscographyProvider.Object, mTagManager.Object, mDialogService.Object ));
		}

		[Test]
		public void CanCreateArtistViewModel() {
			var vm = CreateSut();

			Assert.IsNull( vm.Artist );
		}
	}
}
