using System;
using NUnit.Framework;
using ReusableBits.Patterns;

namespace ReusableBits.Tests.Patterns {
	public interface ITestClass {
		void	SingletonMethod();
	}

	public class TestClass : ITestClass {
		public static int	CallCount { get; private set; }
		
		public void SingletonMethod() {
			CallCount++;
		}

		public static void ResetCallCount() {
			CallCount = 0;
		}
	}

	public class ReplacementClass : ITestClass {
		public int	CallCount { get; private set; }
		
		public void SingletonMethod() {
			CallCount++;
		}
	}

	public class TestSingleton : SingletonBase<ITestClass, TestClass> { }

	internal class TestableSingleton : TestSingleton {
		public static void ResetDefault() {
			mDefault = new Lazy<ITestClass>( InternalCreateDefault );
			Current = null;
			DefaultCreator = null;
		}
	}

	[TestFixture]
	public class SingletonBaseTests {
		[SetUp]
		public void Setup() {
			TestClass.ResetCallCount();
			TestableSingleton.ResetDefault();
		}

		[Test]
		public void CanUseDefaultSingleton() {
			TestableSingleton.Current.SingletonMethod();
	
			Assert.IsTrue( TestClass.CallCount == 1 );
		}

		[Test]
		public void CanReplaceSingleton() {
			var replacement = new ReplacementClass();

			TestableSingleton.Current = replacement;
			TestableSingleton.Current.SingletonMethod();

			Assert.IsTrue( replacement.CallCount == 1 );
			Assert.IsTrue( TestClass.CallCount == 0 );
		}

		[Test]
		public void CanReplaceDefaultCreator() {
			var replacement = new ReplacementClass();

			TestableSingleton.DefaultCreator = () => replacement;
			TestableSingleton.Current.SingletonMethod();

			Assert.IsTrue( replacement.CallCount == 1 );
			Assert.IsTrue( TestClass.CallCount == 0 );
		}

		[Test]
		public void SingletonIsReused() {
			TestableSingleton.Current.SingletonMethod();
			TestableSingleton.Current.SingletonMethod();

			Assert.IsTrue( TestClass.CallCount == 2 );
		}

		[Test]
		public void ReplacementSingletonIsReused() {
			var replacement = new ReplacementClass();
			TestableSingleton.Current = replacement;
			
			TestableSingleton.Current.SingletonMethod();
			TestableSingleton.Current.SingletonMethod();

			Assert.IsTrue( replacement.CallCount == 2 );
		}
	}
}
