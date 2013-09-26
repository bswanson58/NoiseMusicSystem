using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Noise.Core.PlayQueue;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.TestSupport.Mocking;

namespace Noise.Core.Tests.PlayQueue {
	[TestFixture]
	public class PlayControllerTests {
		internal class TestableStorageFileProcessor : Testable<PlayController> { }

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

		private TestableStorageFileProcessor CreateSut() {
			var retValue = new TestableStorageFileProcessor();

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
	}
}
