using System;
using JasonRoberts.FeatureToggle;
using Moq;
using NUnit.Framework;

namespace FeatureToggle.Integration.Tests {
	[TestFixture]
	public class EnabledOnOrAfterDateFeatureToggleTests {
		[Test]
		public void ShouldBeEnabledOnDate() {
			var mockDate = new Mock<INowDateAndTime>();
			mockDate.SetupGet( x => x.Now ).Returns( new DateTime( 2000, 1, 1, 0, 0, 0, 0 ) );
			var sut = new NewYearsDay2000 { NowProvider = mockDate.Object };

			Assert.IsTrue( sut.FeatureEnabled );
		}


		[Test]
		public void ShouldBeEnabledAfterDate() {
			var mockDate = new Mock<INowDateAndTime>();

			mockDate.SetupGet( x => x.Now ).Returns( new DateTime( 2000, 1, 1, 0, 0, 0, 1 ) );

			var sut = new NewYearsDay2000 { NowProvider = mockDate.Object };

			Assert.IsTrue( sut.FeatureEnabled );
		}


		[Test]
		public void ShouldBeDisabledBeforeDate() {
			var mockDate = new Mock<INowDateAndTime>();
			mockDate.SetupGet( x => x.Now ).Returns(
				new DateTime( 2000, 1, 1, 0, 0, 0, 0 ).Subtract( TimeSpan.FromMilliseconds( 1 ) ) );

			var sut = new NewYearsDay2000 { NowProvider = mockDate.Object };

			Assert.IsFalse( sut.FeatureEnabled );
		}


		private class NewYearsDay2000 : EnabledOnOrAfterDateFeatureToggle { }

	}
}