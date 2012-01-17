using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions.EventMonitoring;
using Moq;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;
using Noise.UI.Tests.MockingEventAggregator;
using Noise.UI.Tests.Support;
using Noise.UI.ViewModels;

namespace Noise.UI.Tests.ViewModels {
	[TestFixture]
	public class ArtistViewModelTests {
		private AutoMockingEventAggregator		mEvents;
		private Mock<ILog>						mDummyLog;
		private Mock<IArtistProvider>			mArtistProvider;
		private Mock<IAlbumProvider>			mAlbumProvider;
		private Mock<IDiscographyProvider>		mDiscographyProvider;
		private Mock<ITagManager>				mTagManager;
		private Mock<IDialogService>			mDialogService;
		private TaskScheduler					mTaskScheduler;
			
		[SetUp]
		public void Setup() {
			mDummyLog = new Mock<ILog>();
			NoiseLogger.Current = mDummyLog.Object;

			mEvents = new AutoMockingEventAggregator();
			mArtistProvider = new Mock<IArtistProvider>();
			mAlbumProvider = new Mock<IAlbumProvider>();
			mDiscographyProvider = new Mock<IDiscographyProvider>();
			mTagManager = new Mock<ITagManager>();
			mDialogService = new Mock<IDialogService>();

			mTaskScheduler = new CurrentThreadTaskScheduler();

			mTagManager.Setup( m => m.GetGenre( It.IsAny<long>())).Returns( new DbGenre( 1 ) { Name = "test genre" } );

			SynchronizationContext.SetSynchronizationContext( new SynchronizationContext());
		}

		private ArtistViewModel CreateSut() {
			return( new ArtistViewModel( mEvents, mArtistProvider.Object, mAlbumProvider.Object, mDiscographyProvider.Object,
										 mTagManager.Object, mDialogService.Object ) 
										 { TaskHandler = new TaskHandler<ArtistSupportInfo>( mTaskScheduler, mTaskScheduler )});
		}

		[Test]
		public void CanCreateArtistViewModel() {
			var vm = CreateSut();

			Assert.IsNull( vm.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtist() {
			var artist = new DbArtist { Name = "artist name" };
			var sut = CreateSut();

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist ).Verifiable( "GetArtist not called." );

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			mArtistProvider.Verify();
			Assert.IsNotNull( sut.Artist );
		}

		[Test]
		public void ArtistFocusShouldRequestArtistSupportInfo() {
			var artist = new DbArtist { Name = "artist name" };
			
			CreateSut();

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns((ArtistSupportInfo)null ).Verifiable();

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			mArtistProvider.Verify();
		}

		[Test]
		public void ArtistFocusShouldRequestDiscography() {
			var artist = new DbArtist { Name = "artist name" };
			
			CreateSut();

			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo );
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>()))
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ));
			mDiscographyProvider.Setup( m => m.GetDiscography( It.IsAny<long>())).Returns((DataProviderList<DbDiscographyRelease>)null ).Verifiable();

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			DispatcherPump.DoEvents();

			mDiscographyProvider.Verify();
		}

		[Test]
		public void ArtistFocusShouldTriggerPropertyChanges() {
			var artist = new DbArtist { Name = "artist name" };
			var sut = CreateSut();
			sut.MonitorEvents();

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			DispatcherPump.DoEvents();

			sut.ShouldRaisePropertyChangeFor( m => m.Artist );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistValid );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistWebsite );
		}

		[Test]
		public void ArtistSupportInfoShouldTriggerPropertyChanges() {
			var artist = new DbArtist { Name = "artist name" };
			var sut = CreateSut();
			sut.MonitorEvents();

			var dbTextInfo = new DbTextInfo( 1, ContentType.Biography );
			var biography = new TextInfo( dbTextInfo );
			var dbArtwork = new DbArtwork( 1, ContentType.ArtistPrimaryImage );
			var artistImage = new Artwork( dbArtwork );
			var	similarArtists = new DbAssociatedItemList( 1, ContentType.SimilarArtists );
			var topAlbums = new DbAssociatedItemList( 1, ContentType.TopAlbums );
			var bandMembers = new DbAssociatedItemList( 1, ContentType.BandMembers );

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>()))
				.Returns( new ArtistSupportInfo( biography, artistImage, similarArtists, topAlbums, bandMembers ));

			var databaseShell = new Mock<IDatabaseShell>();
			var discoList = new List<DbDiscographyRelease>();
			var discography = new DataProviderList<DbDiscographyRelease>( databaseShell.Object, discoList );
			mDiscographyProvider.Setup( m => m.GetDiscography( It.IsAny<long>())).Returns( discography );

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			sut.ShouldRaisePropertyChangeFor( m => m.ArtistImage, "ArtistImage" );
			sut.ShouldRaisePropertyChangeFor( m => m.ArtistBio, "ArtistBio" );
			sut.ShouldRaisePropertyChangeFor( m => m.BandMembers, "BandMembers" );
			sut.ShouldRaisePropertyChangeFor( m => m.Discography, "Discography" );
			sut.ShouldRaisePropertyChangeFor( m => m.TopAlbums, "TopAlbums" );
			sut.ShouldRaisePropertyChangeFor( m => m.SimilarArtist, "SimilarArtist" );
		}

		[Test]
		public void SameArtistFocusShouldNotRetrieveArtistAgain() {
			var artist = new DbArtist { Name = "artist name" };
	
			CreateSut();

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns((ArtistSupportInfo)null );

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );
			artistFocusEvent.Publish( artist );

			mArtistProvider.Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Once(), "GetArtistSupportInfo request" );
		}

		[Test]
		public void AlbumFocusShouldRequestArtist() {
			var artist = new DbArtist();
			var album = new DbAlbum { Artist = artist.DbId };

			mArtistProvider.Setup( m => m.GetArtistForAlbum( It.Is<DbAlbum>( p => p.Artist == artist.DbId ))).Returns( artist ).Verifiable();

			CreateSut();

			var albumFocusEvent = mEvents.GetEvent<Events.AlbumFocusRequested>();
			albumFocusEvent.Publish( album );

			mArtistProvider.Verify();
		}

		[Test]
		public void SecondAlbumFocusShouldNotRequestArtist() {
			var artist = new DbArtist();
			var album1 = new DbAlbum { Artist = artist.DbId, Name = "first album" };
			var album2 = new DbAlbum { Artist = artist.DbId, Name = "second album" };

			mArtistProvider.Setup( m => m.GetArtistForAlbum( It.Is<DbAlbum>( p => p.Artist == artist.DbId ))).Returns( artist );

			CreateSut();

			var albumFocusEvent = mEvents.GetEvent<Events.AlbumFocusRequested>();
			albumFocusEvent.Publish( album1 );
			albumFocusEvent.Publish( album2 );

			mArtistProvider.Verify( m => m.GetArtistForAlbum( It.IsAny<DbAlbum>()), Times.Exactly( 1 ));
		}

		[Test]
		public void ArtistInfoUpdateEventShouldRetrieveArtistInfo() {
			var artist = new DbArtist();

			mArtistProvider.Setup( m => m.GetArtist( It.IsAny<long>())).Returns( artist );
			mArtistProvider.Setup( m => m.GetArtistSupportInfo( It.IsAny<long>())).Returns((ArtistSupportInfo)null );

			CreateSut();

			var artistFocusEvent = mEvents.GetEvent<Events.ArtistFocusRequested>();
			artistFocusEvent.Publish( artist );

			var infoUpdateEvent = mEvents.GetEvent<Events.ArtistContentUpdated>();
			infoUpdateEvent.Publish( artist );

			mArtistProvider.Verify( m => m.GetArtistSupportInfo( It.IsAny<long>()), Times.Exactly( 2 ), "GetArtistSupportInfo request" );
		}
	}
}
