using FluentAssertions;
using FluentAssertions.EventMonitoring;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
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

			mTagManager.Setup( m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( new DbArtist { Name = "test artist" });
		}
		private ArtistViewModel CreateSut() {
			return( new ArtistViewModel( mEvents, mArtistProvider.Object, mAlbumProvider.Object, mDiscographyProvider.Object, mTagManager.Object, mDialogService.Object ));
		}

		[Test]
		public void CanCreateArtistViewModel() {
			var vm = CreateSut();

			Assert.IsNull( vm.Artist );
		}

		[Test]
		public void ShouldRespondToArtistFocus() {
			var artist = new DbArtist { Name = "artist name" };
			var sut = CreateSut();
			sut.MonitorEvents();
			sut.PropertyChanged += OnPropertyChanged;


			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

//			artist.ShouldHave().AllProperties().EqualTo( sut.Artist );
//			sut.ShouldRaisePropertyChangeFor( m => m.ArtistValid );
		}

		private void OnPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {
			var propertyName = e.PropertyName;
		}
	}
}
