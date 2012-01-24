using System;
using FluentAssertions;
using Moq;
using StructureMap.AutoMocking;

namespace ReusableBits.TestSupport.Mocking {
	public class Testable<TClassUnderTest> where TClassUnderTest : class {
		private	readonly MoqAutoMocker<TClassUnderTest> mAutoMocker;

		public Testable() {
			 mAutoMocker = new MoqAutoMocker<TClassUnderTest>();
		}

		public Testable( Action<Testable<TClassUnderTest>> setup ) :
			this() {
			setup( this );
		}

		public Mock<TDependencyToMock> Mock<TDependencyToMock>() where TDependencyToMock : class {
			var a = mAutoMocker.Get<TDependencyToMock>();

			return Moq.Mock.Get( a );
		}

		public void Inject<T>( T type ) {
			mAutoMocker.Inject( type );
		}

		public void InjectArray<T>( T[] types ) {
			mAutoMocker.InjectArray( types );
		}

		public virtual TClassUnderTest ClassUnderTest {
			get { return mAutoMocker.ClassUnderTest; }
		}

		public void CreateClassUnderTest() {
			var sut = ClassUnderTest;

			sut.Should().NotBeNull();
		}
	}
}
