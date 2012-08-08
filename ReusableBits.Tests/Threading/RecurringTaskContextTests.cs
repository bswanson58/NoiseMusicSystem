using System;
using FluentAssertions;
using NUnit.Framework;
using ReusableBits.Support;
using ReusableBits.Threading;

namespace ReusableBits.Tests.Threading {
	[TestFixture]
	public class RecurringTaskContextTests {
		private readonly DateTime		mNow;
		private readonly RecurringTask	mTask;
		private long					mTaskExecutedCount;

		public RecurringTaskContextTests() {
			mNow = new DateTime( 2000, 1, 2, 3, 4, 5, 6 );
			mTask = new RecurringTask( TaskAction );
		}

		private void TaskAction() {
			mTaskExecutedCount++;
		}

		[SetUp]
		public void Setup() {
			TimeProvider.Now = () => mNow;
			mTaskExecutedCount = 0;
		}

		private RecurringTaskContext CreateSut() {
			return( new RecurringTaskContext( mTask ));
		}

		[Test]
		public void NewTaskContextShouldBeNotReady() {
			var sut = CreateSut();

			sut.TaskState.Should().Be( RecurringTaskState.NotReady );
		}

		[Test]
		public void TaskContextShouldReturnTask() {
			var sut = CreateSut();

			sut.Task.Should().Be( mTask );
		}

		[Test]
		public void UpdateExecutionTimeShouldSetStateReady() {
			var sut = CreateSut();
			var startTime = new DateTime( mNow.Ticks + 100 );

			mTask.TaskSchedule.StartAt( startTime );
			sut.UpdateNextExecutionTime();

			sut.TaskState.Should().Be( RecurringTaskState.Ready );
		}

		[Test]
		public void TaskContextShouldCalculateExecutionTime() {
			var sut = CreateSut();
			var startTime = new DateTime( mNow.Ticks + 100 );

			mTask.TaskSchedule.StartAt( startTime );
			sut.UpdateNextExecutionTime();
			sut.NextExecutionTime.Should().Be( startTime );
		}

		[Test]
		public void UpdateExecutionTimeWithExpiredTaskShouldSetStateCompleted() {
			var sut = CreateSut();
			var endTime = new DateTime( mNow.Ticks - 100 );

			mTask.TaskSchedule.EndAt( endTime );
			sut.UpdateNextExecutionTime();

			sut.TaskState.Should().Be( RecurringTaskState.Completed );
		}

		[Test]
		public void ExecuteTaskShouldExecuteTaskOnce() {
			var sut = CreateSut();

			sut.ExecuteTask();

			mTaskExecutedCount.Should().Be( 1 );
		}

		[Test]
		public void ExecuteTaskShouldUpdateExecutionTime() {
			var sut = CreateSut();
			var interval = new TimeSpan( 200 );

			sut.Task.TaskSchedule.Interval( interval );
			sut.ExecuteTask();

			var nextExecutionTime = sut.Task.TaskSchedule.CalculateNextExecutionTime();

			nextExecutionTime.Should().Be( mNow + interval );
		}
	}
}
