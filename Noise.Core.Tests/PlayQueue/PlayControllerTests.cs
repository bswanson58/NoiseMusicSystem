using System.Reactive.Linq;
using System.Reactive.Subjects;
using Caliburn.Micro;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.PlaySupport;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;
using ILog = Noise.Infrastructure.Interfaces.ILog;

namespace Noise.Core.Tests.PlayQueue {
	[TestFixture]
	public class PlayControllerTests {
		internal class TestablePlayController : Testable<PlayController> { }

		private const int						cFirstPlaybackChannel = 1;

		private	Subject<AudioChannelStatus>		mChannelStatusSubject;
		private Subject<AudioLevels>			mAudioLevelsSubject;
		private Subject<StreamInfo>				mAudioStreamInfoSubject;

		[SetUp]
		public void Setup() {
			mChannelStatusSubject = new Subject<AudioChannelStatus>();
			mAudioLevelsSubject = new Subject<AudioLevels>();
			mAudioStreamInfoSubject = new Subject<StreamInfo>();

			NoiseLogger.Current = new Mock<ILog>().Object;
		}

		private TestablePlayController CreateSut() {
			var retValue = new TestablePlayController();

			retValue.Mock<IAudioPlayer>().Setup( m => m.AudioLevelsChange ).Returns( mAudioLevelsSubject.AsObservable());
			retValue.Mock<IAudioPlayer>().Setup( m => m.AudioStreamInfoChange ).Returns( mAudioStreamInfoSubject.AsObservable());
			retValue.Mock<IAudioPlayer>().Setup( m => m.ChannelStatusChange ).Returns( mChannelStatusSubject.AsObservable());

			retValue.ClassUnderTest.Initialize();

			return( retValue );
		}

		[Test]
		public void CanCreateSut() {
			var sut = CreateSut();

			sut.Should().NotBeNull( "Testable class was not created." );
			sut.ClassUnderTest.Should().NotBeNull( "Class under test was not created." );
		}

		[Test]
		public void NewPlayControllerShouldBeStopped() {
			var sut = CreateSut();

			sut.ClassUnderTest.CurrentStatus.Should().Be( ePlaybackStatus.Stopped, "New PlayController status should be 'Stopped'." );
		}

		[Test]
		public void NewPlayControllerShouldNotBePlayable() {
			var sut = CreateSut();

			sut.ClassUnderTest.CanPlay.Should().BeFalse( "New PlayController should not be able to play." );
			sut.ClassUnderTest.CanPlayNextTrack.Should().BeFalse( "New PlayController should not be able to play next track." );
			sut.ClassUnderTest.CanPlayPreviousTrack.Should().BeFalse( "New PlayController should not be able to play previous track." );
			sut.ClassUnderTest.CanPause.Should().BeFalse( "New PlayController should not be able to pause playback." );
			sut.ClassUnderTest.CanStop.Should().BeFalse( "New PlayController should not be able to stop." );
		}

		[Test]
		public void NewPlayControllerShouldNotHavePlayingTracks() {
			var sut = CreateSut();

			sut.ClassUnderTest.CurrentTrack.Should().BeNull( "New PlayController should not have a current track." );
			sut.ClassUnderTest.NextTrack.Should().BeNull( "New PlayController should not have a next track." );
			sut.ClassUnderTest.PreviousTrack.Should().BeNull( "New PlayController should not have a previous track." );
		}

		[Test]
		public void PlayQueueEventShouldAllowPlaying() {
			var sut = CreateSut();

			sut.Mock<IPlayQueue>().Setup( m => m.IsQueueEmpty ).Returns( false );
			sut.Mock<IPlayQueue>().Setup( m => m.IsTrackQueued( It.IsAny<DbTrack>())).Returns( false );

			sut.ClassUnderTest.Handle( new Events.PlayQueueChanged( sut.Mock<IPlayQueue>().Object ));

			sut.ClassUnderTest.CanPlay.Should().BeTrue( "Tracks in queue should allow play." );
		}

		private void StartFirstTrack( TestablePlayController sut ) {
			sut.Mock<IPlayQueue>().Setup( m => m.IsQueueEmpty ).Returns( false );
			sut.Mock<IPlayQueue>().Setup( m => m.IsTrackQueued( It.IsAny<DbTrack>())).Returns( false );

			sut.Mock<IAudioPlayer>().Setup( m => m.OpenFile( It.IsAny<string>(), It.IsAny<float>())).Returns( cFirstPlaybackChannel );

			var playTrack = new PlayQueueTrack( new DbArtist(), new DbAlbum(), new DbTrack(), new StorageFile(), string.Empty );
			sut.Mock<IPlayQueue>().Setup( m => m.NextTrack ).Returns( playTrack );
			sut.Mock<IPlayQueue>().Setup( m => m.PlayNextTrack() ).Returns( playTrack );

			sut.ClassUnderTest.Handle( new Events.PlayQueueChanged( sut.Mock<IPlayQueue>().Object ));
			mChannelStatusSubject.OnNext( new AudioChannelStatus( cFirstPlaybackChannel, ePlaybackStatus.TrackStart ));
		}

		[Test]
		public void AddingFirstTrackShouldPlayTrack() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			sut.Mock<IAudioPlayer>().Verify( m => m.Play( cFirstPlaybackChannel ), Times.Once());
		}

		[Test]
		public void AudioPlayStartedShouldSetPlayingStatus() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			sut.ClassUnderTest.CurrentStatus.Should().Be( ePlaybackStatus.TrackStart );
		}

		private Events.PlaybackStatusChanged IsPlaybackStatus( ePlaybackStatus status ) {
			return( Match.Create<Events.PlaybackStatusChanged>( e => e.Status == status ));
		}

		[Test]
		public void PlayingTrackShouldFirePlaybackStarted() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			sut.Mock<IEventAggregator>().Verify( m => m.Publish( IsPlaybackStatus( ePlaybackStatus.TrackStart )), Times.Once(), "PlayStart did not fire ePlaybackStatus.TrackStart");
		}

		[Test]
		public void StopShouldStopAudioPlayback() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			sut.ClassUnderTest.Stop();
			mChannelStatusSubject.OnNext( new AudioChannelStatus( cFirstPlaybackChannel, ePlaybackStatus.Stopped ));

			sut.ClassUnderTest.CurrentStatus.Should().Be( ePlaybackStatus.Stopped, "Stopped Audio Status did not set status to Stopped" );
		}

		[Test]
		public void StopShouldFirePlaybackStopped() {
			var sut = CreateSut();

			sut.Mock<IPlayQueue>().Setup( m => m.IsQueueEmpty ).Returns( false );

			StartFirstTrack( sut );
			sut.ClassUnderTest.Stop();
			mChannelStatusSubject.OnNext( new AudioChannelStatus( cFirstPlaybackChannel, ePlaybackStatus.Stopped ));

			sut.Mock<IEventAggregator>().Verify( m => m.Publish( IsPlaybackStatus( ePlaybackStatus.Stopped )), Times.AtLeast( 3 ), "Stop did not fire ePlaybackStatus.Stopped" );
		}

		[Test]
		public void StartingPlayerShouldSetCurrentTrack() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			sut.ClassUnderTest.CurrentTrack.Should().NotBeNull( "CurrentTrack was not set on playback." );
		}

		[Test]
		public void TrackEndShouldStopPlaybackWithEmptyQueue() {
			var sut = CreateSut();

			StartFirstTrack( sut );
			sut.Mock<IPlayQueue>().Setup( m => m.PlayNextTrack()).Returns( default( PlayQueueTrack ));

			mChannelStatusSubject.OnNext( new AudioChannelStatus( cFirstPlaybackChannel, ePlaybackStatus.TrackEnd ));

			sut.ClassUnderTest.CurrentStatus.Should().Be( ePlaybackStatus.Stopped, "TrackEnd did not set status to Stopped" );
		}

		[Test]
		public void TrackEndShouldStartNextTrack() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			var nextTrack = new PlayQueueTrack( new DbArtist(), new DbAlbum(), new DbTrack(), new StorageFile(), string.Empty );
			sut.Mock<IPlayQueue>().Setup( m => m.PlayNextTrack() ).Returns( nextTrack );
			sut.Mock<IAudioPlayer>().Setup( m => m.OpenFile( It.IsAny<string>(), It.IsAny<float>())).Returns( 2 );

			mChannelStatusSubject.OnNext( new AudioChannelStatus( cFirstPlaybackChannel, ePlaybackStatus.TrackEnd ));
			mChannelStatusSubject.OnNext( new AudioChannelStatus( 2, ePlaybackStatus.TrackStart ));

			sut.ClassUnderTest.CurrentTrack.Should().Be( nextTrack, "TrackEnd did not start next track playing" );
		}

		[Test]
		public void StartShouldBeAllowedAfterStop() {
			var sut = CreateSut();

			StartFirstTrack( sut );

			var nextTrack = new PlayQueueTrack( new DbArtist(), new DbAlbum(), new DbTrack(), new StorageFile(), string.Empty );
			sut.Mock<IPlayQueue>().Setup( m => m.PlayNextTrack() ).Returns( nextTrack );
			sut.Mock<IAudioPlayer>().Setup( m => m.OpenFile( It.IsAny<string>(), It.IsAny<float>())).Returns( 2 );

			sut.ClassUnderTest.Stop();
			mChannelStatusSubject.OnNext( new AudioChannelStatus( 2, ePlaybackStatus.Stopped ));

			sut.ClassUnderTest.CanPlay.Should().BeTrue( "Play should be allowed if queue is not empty." );
		}
	}
}
