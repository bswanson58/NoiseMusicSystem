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
				CallTimer();
			}
		}

		public void CallTimer() {
			OnTimer( null );
		}
	}

	[TestFixture]
	public class RecurringTaskSchedulerTests {
		private readonly string		mTaskName;
		private TaskScheduler		mTestScheduler;
		private RecurringTask		mTask;
		private readonly DateTime	mNow;
		private long				mTaskExecutionCount;

		public RecurringTaskSchedulerTests() {
			mNow = new DateTime( 2000, 1, 2, 3, 4, 5, 6 );
			mTaskName = "Test Task";
		}

		[SetUp]
		public void Setup() {
			mTestScheduler = new CurrentThreadTaskScheduler();
			mTask = new RecurringTask( TaskAction, mTaskName );
			mTask.TaskSchedule.RepeatCount( 1 );

			mTaskExecutionCount = 0;
			TimeProvider.Now = () => mNow;
		}

		private void TaskAction( RecurringTask task ) {
			mTaskExecutionCount++;
		}

		private TestableRecurringTaskScheduler CreateSut() {
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

		[Test]
		public void FutureTaskIsNotRunBeforeSchedule() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow + new TimeSpan( 0, 1, 0 ));
			sut.AddRecurringTask( mTask );

			TimeProvider.Now = () => mNow + new TimeSpan( 0, 0, 59 );
			sut.CallTimer();

			mTaskExecutionCount.Should().Be( 0 );
		}

		[Test]
		public void FutureTaskIsRunOnSchedule() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow + new TimeSpan( 0, 1, 0 ));
			sut.AddRecurringTask( mTask );

			TimeProvider.Now = () => mNow + new TimeSpan( 0, 0, 59 );
			sut.CallTimer();

			TimeProvider.Now = () => mNow + new TimeSpan( 0, 1, 0 );
			sut.CallTimer();

			mTaskExecutionCount.Should().Be( 1 );
		}

		[Test]
		public void CanRemoveAllTasks() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow + new TimeSpan( 0, 1, 0 ));
			sut.AddRecurringTask( mTask );
			sut.RemoveAllTasks();

			TimeProvider.Now = () => mNow + new TimeSpan( 0, 1, 1 );
			sut.CallTimer();

			mTaskExecutionCount.Should().Be( 0 );
		}

		[Test]
		public void CanRemoveNamedTask() {
			var sut = CreateSut();

			mTask.TaskSchedule.StartAt( mNow + new TimeSpan( 0, 1, 0 ));
			sut.AddRecurringTask( mTask );
			sut.RemoveTask( mTaskName );

			TimeProvider.Now = () => mNow + new TimeSpan( 0, 1, 1 );
			sut.CallTimer();

			mTaskExecutionCount.Should().Be( 0 );
		}

		[Test]
		public void CanRetrieveTask() {
			var sut = CreateSut();

			sut.AddRecurringTask( mTask );

			var retrievedTask = sut.RetrieveTask( mTaskName );
			retrievedTask.Should().Be( mTask );
		}
	}
}
