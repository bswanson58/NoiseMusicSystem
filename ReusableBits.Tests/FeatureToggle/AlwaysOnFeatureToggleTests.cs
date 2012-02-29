using NUnit.Framework;

namespace ReusableBits.Tests.FeatureToggle {
	[TestFixture]
	public class AlwaysOnFeatureToggleTests {
		[Test]
		public void ShouldReturnAlwaysOn() {
			var sut = new MyAlwaysOnFeatureToggle();

			Assert.IsTrue( sut.FeatureEnabled );
		}

		private class MyAlwaysOnFeatureToggle : AlwaysOnFeatureToggle { }
	}
}
