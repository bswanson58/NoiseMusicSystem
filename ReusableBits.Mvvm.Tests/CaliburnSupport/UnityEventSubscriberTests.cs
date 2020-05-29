using Caliburn.Micro;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReusableBits.Mvvm.CaliburnSupport;

namespace ReusableBits.Mvvm.Tests.CaliburnSupport {
	internal class TestUnityEventSubscriber : UnityEventSubscriber {
		public TestUnityEventSubscriber( IEventAggregator eventAggregator ) :
			base( eventAggregator ) { }

		public IBuilderStrategy GetStrategy() {
			return( new EventSubscriberStrategy( mEventAggregator ));
		}
	}

	internal class ClassWithoutEvents {
		public void SomeMethod() { }
	}

	internal class ClassWithEvents : IHandle<ClassWithoutEvents> {
		public void Handle( ClassWithoutEvents message ) { }
	}

	internal class ClassWithSubscriptionDisabled : IHandle<ClassWithoutEvents>, IDisableAutoEventSubscribe {
		public void Handle( ClassWithoutEvents message ) { }
	}

	[TestFixture]
	public class UnityEventSubscriberTests {
		private Mock<IEventAggregator>	mEventAggregator;

		private IBuilderStrategy CreateSut() {
			return( new TestUnityEventSubscriber( mEventAggregator.Object ).GetStrategy());
		}

		[SetUp]
		public void Setup() {
			mEventAggregator = new Mock<IEventAggregator>();
		}

		[Test]
		public void CanCreateExtension() {
			var extension = CreateSut();

			extension.Should().NotBeNull();
		}

		[Test]
		public void NonEventClassShouldNotBeSubscribed() {
			var context = new Mock<IBuilderContext>();
			var testClass = new ClassWithoutEvents();

			context.Setup( m => m.Existing ).Returns( testClass );

			var extension = CreateSut();

			extension.PostBuildUp( context.Object );
			mEventAggregator.Verify( m => m.Subscribe( testClass ), Times.Never());
		}

		[Test]
		public void EventClassShouldBeSubscribed() {
			var context = new Mock<IBuilderContext>();
			var testClass = new ClassWithEvents();

			context.Setup( m => m.Existing ).Returns( testClass );

			var extension = CreateSut();

			extension.PostBuildUp( context.Object );
			mEventAggregator.Verify( m => m.Subscribe( testClass ), Times.Once());
		}

		[Test]
		public void EventsDisabledDoesNotSubscribe() {
			var context = new Mock<IBuilderContext>();
			var testClass = new ClassWithSubscriptionDisabled();

			context.Setup( m => m.Existing ).Returns( testClass );

			var extension = CreateSut();

			extension.PostBuildUp( context.Object );
			mEventAggregator.Verify( m => m.Subscribe( testClass ), Times.Never());
		}
	}
}
