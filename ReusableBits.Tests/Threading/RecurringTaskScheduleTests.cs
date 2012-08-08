using System;
using FluentAssertions;
using NUnit.Framework;
using ReusableBits.Support;
using ReusableBits.Threading;

namespace ReusableBits.Tests.Threading {
	[TestFixture]
	public class RecurringTaskScheduleTests {
		private readonly DateTime	mNow;

		public RecurringTaskScheduleTests() {
			mNow = new DateTime( 2010, 10, 30, 6, 7, 8, 9 );
		}

		[SetUp]
		public void Setup() {
			TimeProvider.Now = () => ( mNow );
		}

		private RecurringTaskSchedule CreateSut() {
			return( new RecurringTaskSchedule());
		}

		[Test]
		public void CanSetStartTime() {
			var sut = CreateSut();
			var startTime = new DateTime( 2010, 10, 30, 11, 17, 29, 12 );

			sut.StartAt( startTime );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.HasValue.Should().BeTrue();
		}

		[Test]
		public void CanSetEndTime() {
			var	sut = CreateSut();
			var endTime = mNow + new TimeSpan( 100 );

			sut.EndAt( endTime );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.HasValue.Should().BeTrue();
		}

		[Test]
		public void FutureStartTimeReturnsStartTime() {
			var sut = CreateSut();
			var startTime = mNow + new TimeSpan( 1000 );

			sut.StartAt( startTime );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( startTime );
			}
		}

		[Test]
		public void PastStartTimeShouldReturnNowForFirstExection() {
			var sut = CreateSut();
			var startTime = mNow - new TimeSpan( 1000 );

			sut.StartAt( startTime );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( mNow );
			}
		}

		[Test]
		public void PastEndTimeShouldReturnNull() {
			var sut = CreateSut();
			var endTime = mNow - new TimeSpan( 100 );

			sut.EndAt( endTime );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void FirstExecutionBetweenTimeShouldBeNow() {
			var sut = CreateSut();
			var startTime = mNow - new TimeSpan( 100 );
			var endTime = mNow + new TimeSpan( 100 );

			sut.StartAt( startTime ).EndAt( endTime );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( mNow );
			}
		}

		[Test]
		public void FirstExecutionOutsideShouldBeNull() {
			var sut = CreateSut();
			var startTime = mNow - new TimeSpan( 200 );
			var endTime = mNow - new TimeSpan( 100 );

			sut.StartAt( startTime ).EndAt( endTime );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void NextExecutionShouldEqualInterval() {
			var sut = CreateSut();
			var lastExecutionStart = mNow - new TimeSpan( 100 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );
			var intervalTime = new TimeSpan( 200 );

			sut.Interval( intervalTime );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( lastExecutionStart + intervalTime );
			}
		}

		[Test]
		public void NextExecutionShouldEqualDelay() {
			var sut = CreateSut();
			var lastExecutionStart = mNow - new TimeSpan( 100 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );
			var delayTime = new TimeSpan( 200 );

			sut.Delay( delayTime );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( lastExecutionEnd + delayTime );
			}
		}

		[Test]
		public void FirstExecutionIntervalPastEndTimeShouldReturnNow() {
			var sut = CreateSut();
			var endTime = mNow + new TimeSpan( 100 );
			var interval = new TimeSpan( 200 );

			sut.EndAt( endTime ).Interval( interval );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( mNow );
			}
		}

		[Test]
		public void NextExecutionIntervalPastEndTimeShouldReturnNull() {
			var sut = CreateSut();
			var endTime = mNow + new TimeSpan( 100 );
			var interval = new TimeSpan( 200 );
			var lastExecutionStart = mNow - new TimeSpan( 50 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );

			sut.EndAt( endTime ).Interval( interval );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void FirstExecutionDelayPastEndTimeShouldReturnNow() {
			var sut = CreateSut();
			var endTime = mNow + new TimeSpan( 100 );
			var delay = new TimeSpan( 200 );

			sut.EndAt( endTime ).Delay( delay );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
			if( executionTime.HasValue ) {
				executionTime.Value.Should().Be( mNow );
			}
		}

		[Test]
		public void NextExecutionDelayPastEndTimeShouldReturnNull() {
			var sut = CreateSut();
			var endTime = mNow + new TimeSpan( 100 );
			var delay = new TimeSpan( 200 );
			var lastExecutionStart = mNow - new TimeSpan( 50 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );

			sut.EndAt( endTime ).Delay( delay );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void ZeroRepeatCountShouldReturnNull() {
			var sut = CreateSut();

			sut.RepeatCount( 0 );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void NonZeroRepeatCountShouldReturnValue() {
			var sut = CreateSut();

			sut.RepeatCount( 1 );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
		}

		[Test]
		public void ExceededRepeatCountShouldReturnNull() {
			var sut = CreateSut();
			var lastExecutionStart = mNow - new TimeSpan( 50 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );

			sut.RepeatCount( 1 );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void NonExceededRepeatCountShouldReturnTime() {
			var sut = CreateSut();
			var lastExecutionStart = mNow - new TimeSpan( 50 );
			var lastExecutionEnd = mNow - new TimeSpan( 50 );

			sut.RepeatCount( 2 );
			sut.UpdateLastExecutionTime( lastExecutionStart, lastExecutionEnd );
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();
		}

		[Test]
		public void PauseShouldReturnNullTime() {
			var sut = CreateSut();

			sut.StartAt( mNow );
			sut.Pause();
			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void ResumeShouldReturnTime() {
			var sut = CreateSut();

			sut.StartAt( mNow );
			sut.Pause();
			sut.CalculateNextExecutionTime();
			sut.CalculateNextExecutionTime();
			sut.Resume();

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().HaveValue();

		}

		[Test]
		public void CanSetStartFromSecondsInterval() {
			var sut = CreateSut();

			sut.StartAt( RecurringInterval.FromSeconds( 3 ));

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().Be( mNow + new TimeSpan( 0, 0, 3 ));
		}

		[Test]
		public void CanSetEndTimeFromMsInterval() {
			var sut = CreateSut();

			sut.EndAt( RecurringInterval.FromMilliseconds( 100 ));
			TimeProvider.Now = () => mNow + new TimeSpan( 0, 0, 0, 0, 120 );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().NotHaveValue();
		}

		[Test]
		public void CanSetIntervalFromIntervalMinutes() {
			var sut = CreateSut();

			sut.Interval( RecurringInterval.FromMinutes( 3 ));
			sut.UpdateLastExecutionTime( mNow, mNow );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().Be( mNow + new TimeSpan( 0, 3, 0));
		}

		[Test]
		public void CanSetDelayFromIntervalHours() {
			var sut = CreateSut();

			sut.Delay( RecurringInterval.FromHours( 4 ));
			sut.UpdateLastExecutionTime( mNow, mNow );

			var executionTime = sut.CalculateNextExecutionTime();

			executionTime.Should().Be( mNow + new TimeSpan( 4, 0, 0));
		}
	}
}
