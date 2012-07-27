using System;
using FluentAssertions;
using NUnit.Framework;
using ReusableBits.TestSupport.Threading;

namespace ReusableBits.Tests {
	[TestFixture]
	public class TaskHandlerTests {
		private CurrentThreadTaskScheduler	mTaskScheduler;
		private int							mActionCount;
		private int							mCompletionCount;
		private int							mErrorCount;

		[SetUp]
		public void Setup() {
			mTaskScheduler = new CurrentThreadTaskScheduler();

			mActionCount = 0;
			mCompletionCount = 0;
			mErrorCount = 0;
		}

		private TaskHandler CreateSut() {
			return( new TaskHandler( mTaskScheduler, mTaskScheduler ));
		}

		private void ActionTask() {
			mActionCount++;
		}

		private void ActionCompletion() {
			mCompletionCount++;
		}

		private void ErrorTask( Exception ex ) {
			mErrorCount++;
		}

		[Test]
		public void CanExecuteTask() {
			var sut = CreateSut();

			sut.StartTask( ActionTask, ActionCompletion, ErrorTask );

			mActionCount.Should().Be( 1 );
		}

		[Test]
		public void ExecuteTaskShouldCompleteTask() {
			var sut = CreateSut();

			sut.StartTask( ActionTask, ActionCompletion, ErrorTask );

			mCompletionCount.Should().Be( 1 );
		}

		[Test]
		public void ExecuteTaskShouldNotExecuteErrorTask() {
			var sut = CreateSut();

			sut.StartTask( ActionTask, ActionCompletion, ErrorTask );

			mErrorCount.Should().Be( 0 );
		}

		[Test]
		public void ExceptionTaskShouldNotCallCompletion() {
			var sut = CreateSut();

			sut.StartTask( () => { throw new Exception(); }, ActionCompletion, ErrorTask );

			mCompletionCount.Should().Be( 0 );
		}

		[Test]
		public void ExceptionTaskShouldCallErrorTask() {
			var sut = CreateSut();

			sut.StartTask( () => { throw new Exception(); }, ActionCompletion, ErrorTask );

			mErrorCount.Should().Be( 1 );
		}

		internal class TaskClass {
			public string	TaskData { get; set; }
		}

		private TaskHandler<TaskClass> CreateGenericSut() {
			return( new TaskHandler<TaskClass>( mTaskScheduler, mTaskScheduler ));
		} 

		private TaskClass GenericActionTask() {
			mActionCount++;

			return( new TaskClass());
		}

		private void GenericCompletionTask( TaskClass taskClass ) {
			taskClass.Should().NotBeNull();

			mCompletionCount++;
		}

		[Test]
		public void CanExecuteGenericTask() {
			var sut = CreateGenericSut();

			sut.StartTask( GenericActionTask, GenericCompletionTask, ErrorTask );

			mActionCount.Should().Be( 1 );
		}

		[Test]
		public void ExecuteGenericTaskShouldCompleteTask() {
			var sut = CreateGenericSut();

			sut.StartTask( GenericActionTask, GenericCompletionTask, ErrorTask );

			mCompletionCount.Should().Be( 1 );
		}

		[Test]
		public void ExecuteGenericTaskShouldNotExecuteErrorTask() {
			var sut = CreateGenericSut();

			sut.StartTask( GenericActionTask, GenericCompletionTask, ErrorTask );

			mErrorCount.Should().Be( 0 );
		}

		[Test]
		public void GenericExceptionTaskShouldNotCallCompletion() {
			var sut = CreateGenericSut();

			sut.StartTask( () => { throw new Exception(); }, GenericCompletionTask, ErrorTask );

			mCompletionCount.Should().Be( 0 );
		}

		[Test]
		public void GenericExceptionTaskShouldCallErrorTask() {
			var sut = CreateGenericSut();

			sut.StartTask( () => { throw new Exception(); }, GenericCompletionTask, ErrorTask );

			mErrorCount.Should().Be( 1 );
		}
	}
}
