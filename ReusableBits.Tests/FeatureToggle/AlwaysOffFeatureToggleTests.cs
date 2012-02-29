using NUnit.Framework;

namespace JasonRoberts.FeatureToggle.Tests {
	[TestFixture]
	public class AlwaysOffFeatureToggleTests {
		[Test]
		public void ShouldReturnAlwaysOff() {
			var sut = new MyAlwaysOffFeatureToggle();

			Assert.IsFalse( sut.FeatureEnabled );
		}

		private class MyAlwaysOffFeatureToggle : AlwaysOffFeatureToggle { }
	}
}
