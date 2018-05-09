using System;
using FluentAssertions;
using NUnit.Framework;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using Noise.UI.Models;
using ReusableBits.TestSupport.Mocking;

namespace Noise.UI.Tests.Models {
	public class TestableSelectionStateModel : Testable<SelectionStateModel> { }

	public class ChangeObserver<T> {
		public	int		ChangeCount { get; private set; }

		public void SetObserver( IObservable<T> source ) {
			source.Subscribe( OnSourceTrigger);
		}

		private void OnSourceTrigger( T args ) {
			ChangeCount++;
		}
	}

	[TestFixture]
	public class SelectionStateModelTests {
		private ISelectionState CreateSut() {
			return( new TestableSelectionStateModel().ClassUnderTest );
		}

		[Test]
		public void CanCreateSut() {
			var sut = CreateSut();

			sut.Should().BeAssignableTo<ISelectionState>( "Unit under test should be of type ISelectionStateModel" );
		}

		[Test]
		public void CurrentArtistShouldInitiallyBeNull() {
			var	sut = CreateSut();

			sut.CurrentArtist.Should().BeNull( "Initial state of CurrentArtist should be null" );
		}

		[Test]
		public void CurrentAlbumInitiallyShouldBeNull() {
			var sut = CreateSut();

			sut.CurrentAlbum.Should().BeNull( "Initial state of CurrentAlbum should be null" );
		}

		[Test]
		public void ArtistFocusShouldSetArtist() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			sut.CurrentArtist.Should().Be( artist );
		}

		[Test]
		public void AlbumFocusRequestShouldSetAlbum() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId )).Returns( album );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.CurrentAlbum.Should().Be( album );
		}

		[Test]
		public void AlbumFocusRequestShouldSetArtist() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album name" };

		    testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId )).Returns( album );

			var sut = testable.ClassUnderTest;
			sut.Handle( new Events.AlbumFocusRequested( album ));

			sut.CurrentArtist.Should().Be( artist );
		}

		[Test]
		public void ArtistFocusShouldTriggerArtistChanged() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId ) ).Returns( artist );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			sut.Handle( new Events.ArtistFocusRequested( artist.DbId ));

			observer.ChangeCount.Should().Be( 1, "Artist changed should be triggered once for ArtistFocusRequested" );
		}

		[Test]
		public void AlbumFocusShouldTriggerAlbumChanged() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album name" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId ) ).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId ) ).Returns( album );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbAlbum>();

			observer.SetObserver( sut.CurrentAlbumChanged );

			sut.Handle( new Events.AlbumFocusRequested( album ));

			observer.ChangeCount.Should().Be( 1, "Album changed should be triggered once for AlbumFocusRequested" );
		}

		[Test]
		public void SecondAlbumRequestForArtistShouldNotTriggerArtistChanged() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album1 = new DbAlbum { Artist = artist.DbId, Name = "album one" };
			var album2 = new DbAlbum { Artist = artist.DbId, Name = "album two" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId ) ).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album1.DbId ) ).Returns( album1 );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album2.DbId ) ).Returns( album2 );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			sut.Handle( new Events.AlbumFocusRequested( album1 ));
			sut.Handle( new Events.AlbumFocusRequested( album2 ));

			observer.ChangeCount.Should().Be( 1, "Second album request for artist should not trigger artist changed." );
		}

		[Test]
		public void SecondAlbumRequestForDifferentArtistShouldTriggerArtistChanged() {
			var testable = new TestableSelectionStateModel();
			var artist1 = new DbArtist { Name = "artist one" };
			var artist2 = new DbArtist { Name = "artist two" };
			var album1 = new DbAlbum { Artist = artist1.DbId, Name = "album one" };
			var album2 = new DbAlbum { Artist = artist2.DbId, Name = "album two" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist1.DbId ) ).Returns( artist1 );
			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist2.DbId ) ).Returns( artist2 );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album1.DbId ) ).Returns( album1 );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album2.DbId ) ).Returns( album2 );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			sut.Handle( new Events.AlbumFocusRequested( album1 ));
			sut.Handle( new Events.AlbumFocusRequested( album2 ));

			observer.ChangeCount.Should().Be( 2, "Second album request for different artist should trigger artist changed." );
		}

		[Test]
		public void PlaybackTrackEventTriggersArtistChanged() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album one" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId )).Returns( album );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			var track = new DbTrack { Album = album.DbId, Name = "track name" };
			var playbackTrack = new PlayQueueTrack( artist, album, track, null, string.Empty );

			sut.SetPlaybackTrackFocus( true, new TimeSpan( 0, 0, 0, 0 ));
			sut.Handle( new Events.PlaybackTrackStarted( playbackTrack ));

			observer.ChangeCount.Should().Be( 1, "Playback should trigger artist changed" );
		}

		[Test]
		public void PlaybackTrackEventShouldNotTriggersArtistChangedBeforeDelay() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album one" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId )).Returns( album );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			var track = new DbTrack { Album = album.DbId, Name = "track name" };
			var playbackTrack = new PlayQueueTrack( artist, album, track, null, string.Empty );

			sut.SetPlaybackTrackFocus( true, new TimeSpan( 0, 0, 1, 0 ));
			sut.Handle( new Events.PlaybackTrackStarted( playbackTrack ));

			observer.ChangeCount.Should().Be( 0, "Playback should not trigger artist changed before delay" );
		}

		[Test]
		public void PlaybackTrackEventCanBeDisabled() {
			var testable = new TestableSelectionStateModel();
			var artist = new DbArtist { Name = "artist name" };
			var album = new DbAlbum { Artist = artist.DbId, Name = "album one" };

			testable.Mock<IArtistProvider>().Setup( m => m.GetArtist( artist.DbId )).Returns( artist );
			testable.Mock<IAlbumProvider>().Setup( m => m.GetAlbum( album.DbId )).Returns( album );

			var sut = testable.ClassUnderTest;
			var observer = new ChangeObserver<DbArtist>();

			observer.SetObserver( sut.CurrentArtistChanged );

			var track = new DbTrack { Album = album.DbId, Name = "track name" };
			var playbackTrack = new PlayQueueTrack( artist, album, track, null, string.Empty );

			sut.SetPlaybackTrackFocus( false, new TimeSpan( 0, 0, 0, 0 ) );
			sut.Handle( new Events.PlaybackTrackStarted( playbackTrack ) );

			observer.ChangeCount.Should().Be( 0, "Playback focus should be disabled" );
		}
	}
}
