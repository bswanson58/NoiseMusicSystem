using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ReusableBits.Support;
using ReusableBits.TestSupport.Threading;
using ReusableBits.Threading;

namespace ReusableBits.Tests.Threading {
	internal class TestableRecurringTaskScheduler : RecurringTaskScheduler {
		public TestableRecurringTaskScheduler( TaskScheduler taskScheduler, TaskScheduler uiTaskScheduler ) :
			base( taskScheduler, uiTaskScheduler ) { }

		protected override void SetTimer( long startTime ) {
			if(( startTime != Timeout.Infinite ) &&
			   ( startTime < 20 )) {
				OnTimer( null );
			}
		}
	}

	[TestFixture]
	public class RecurringTaskSchedulerTests {
		private TaskScheduler		mTestScheduler;
		private RecurringTask		mTask;
		private readonly DateTime	mNow;
		private long				mTaskExecutionCount;

		public RecurringTaskSchedulerTests() {
			mNow = new DateTime( 2000, 1, 2, 3, 4, 5, 6 );
		}

		[SetUp]
		public void Setup() {
			mTestScheduler = new CurrentThreadTaskScheduler();
			mTask = new RecurringTask( TaskAction );
			mTask.TaskSchedule.RepeatCount( 1 );

			mTaskExecutionCount = 0;
			TimeProvider.Now = () => mNow;
		}

		private void TaskAction() {
			mTaskExecutionCount++;
		}

		private RecurringTaskScheduler CreateSut() {
			return( new TestableRecurringTaskScheduler( mTestScheduler, mTestScheduler ));
		}

		[Test]
		public void CanAddTask() {
			var sut = CreateSut();

			sut.AddRecurringTask( mTask );
		}

		[Test]
		public void AddedTaskIsRun() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow );
			sut.AddRecurringTask( mTask );

			mTaskExecutionCount.Should().Be( 1 );
		}

		[Test]
		public void FutureTaskIsNotRun() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow + new TimeSpan( 0, 1, 0 ));
			sut.AddRecurringTask( mTask );

			mTaskExecutionCount.Should().Be( 0 );
		}
	}
}
