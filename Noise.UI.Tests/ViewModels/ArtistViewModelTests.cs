using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions.EventMonitoring;
using Microsoft.Practices.Prism.Events;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.ViewModels;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.TestSupport.Mocking;
using ReusableBits.TestSupport.MockingEventAggregator;
using ReusableBits.TestSupport.Threading;

namespace Noise.UI.Tests.ViewModels {
	internal class TestableArtistViewModel : Testable<ArtistViewModel> {
		private readonly TaskScheduler			mTaskScheduler;
		private TaskHandler<ArtistSupportInfo>	mTaskHandler;
		public	AutoMockingEventAggregator		EventAggregator { get; private set; }

		public TestableArtistViewModel() {
			// Set tpl tasks to use the current thread only.
			mTaskScheduler = new CurrentThreadTaskScheduler();

			EventAggregator = new AutoMockingEventAggregator();
			Inject<IEventAggregator>( EventAggregator );

			Mock<ITagManager>().Setup(  m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" });
		}

		public override ArtistViewModel ClassUnderTest {
			get {
				var		retValue = base.ClassUnderTest;

				if(( retValue != null ) &&
				   ( mTaskHandler == null )) {
					mTaskHandler = new TaskHandler<ArtistSupportInfo>( mTaskScheduler, mTaskScheduler );
				
					retValue.TaskHandler = mTaskHandler;
				}

				return( retValue );
			}
		}
	}

	[TestFixture]
	public class ArtistViewModelTests {

		[SetUp]
		public void Setup() {
			NoiseLogger.Current = new Mock<ILog>().Object;

			// Set the ui dispatcher to run on the current thread.
			Execute.ResetWithoutDispatcher();
		}

		[Test]
		public void CanCreateArtistViewModel() {
			var sut = new TestableArtistViewModel().ClassUnderTest;

			Assert.IsNull( sut.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist ).Verifiable( "GetArtist not called." );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify();
			Assert.IsNotNull( sut.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtistSupportInfo() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns( (ArtistSupportInfo)null ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify();
		}

		[Test]
		public void ArtistFocusShouldRequestDiscography() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };
			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo );
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>() ) )
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ) );
			testable.Mock<IDiscographyProvider>().Setup( m => m.GetDiscography( It.IsAny<long>() ) ).Returns( (DataProviderList<DbDiscographyRelease>)null ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			DispatcherPump.DoEvents();

			testable.Mock<IDiscographyProvider>().Verify();
		}

		[Test]
		public void ArtistFocusShouldTriggerPropertyChanges() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			DispatcherPump.DoEvents();

			sut.ShouldRaisePropertyChangeFor( m => m.Artist );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistValid );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistWebsite );
		}

		[Test]
		public void ArtistSupportInfoShouldTriggerPropertyChanges() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo );
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>() ) )
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ) );

			var databaseShell = new Mock<IDatabaseShell>();
			var discoList = new List<DbDiscographyRelease>();
			var discography = new DataProviderList<DbDiscographyRelease>( databaseShell.Object, discoList );
			testable.Mock<IDiscographyProvider>().Setup( m => m.GetDiscography( It.IsAny<long>() ) ).Returns( discography );

			var sut = testable.ClassUnderTest;
			sut.MonitorEvents();

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			sut.ShouldRaisePropertyChangeFor( m => m.ArtistImage, "ArtistImage" );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistBio, "ArtistBio" );
			sut.ShouldRaisePropertyChangeFor( m => m.BandMembers, "BandMembers" );
			sut.ShouldRaisePropertyChangeFor( m => m.Discography, "Discography" );
			sut.ShouldRaisePropertyChangeFor( m => m.TopAlbums, "TopAlbums" );
			sut.ShouldRaisePropertyChangeFor( m => m.SimilarArtist, "SimilarArtist" );
		}

		[Test]
		public void SameArtistFocusShouldNotRetrieveArtistAgain() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns( (ArtistSupportInfo)null );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Once(), "GetArtistSupportInfo request" );
		}

		[Test]
		public void AlbumFocusShouldRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.Is<long>( p => p == artist.DbId ))).Returns( artist ).Verifiable();

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album ));

			testable.Mock<IArtistProvider>().Verify();
		}

		[Test]
		public void SecondAlbumFocusShouldNotRequestArtist() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId, Name = "first album" };
			var album2 = new DbAlbum { Artist = artist.DbId, Name = "second album" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.Is<long>( p => p == artist.DbId ))).Returns( artist );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.AlbumFocusRequested( album1 ));
			sut.Handle( new Events.AlbumFocusRequested( album2 ));

			// The artist is requested twice in the current code for each focus request.
			testable.Mock<IArtistProvider>().Verify( m => m.GetArtist( It.IsAny<long>()), Times.Exactly( 2 ));
		}

		[Test]
		public void ArtistInfoUpdateEventShouldRetrieveArtistInfo() {
			var testable = new TestableArtistViewModel();
			var artist = new DbArtist();

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( It.IsAny<long>() ) ).Returns( artist );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns( (ArtistSupportInfo)null );

			var sut = testable.ClassUnderTest;

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			var infoUpdateEvent = testable.EventAggregator.GetEvent<Events.ArtistContentUpdated>();
			infoUpdateEvent.Publish( artist );

			testable.Mock<IArtistProvider>().Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Exactly( 2 ), "GetArtistSupportInfo request" );
		}
	}
}
