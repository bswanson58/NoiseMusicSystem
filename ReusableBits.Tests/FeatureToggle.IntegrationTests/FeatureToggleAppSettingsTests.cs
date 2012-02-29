using JasonRoberts.FeatureToggle;
using NUnit.Framework;

namespace FeatureToggle.Integration.Tests {
	[TestFixture]
	public class FeatureToggleAppSettingsTests {
		[Test]
		public void ShouldUseConvetionToGetValueFromAppConfig() {
			Assert.IsTrue( new ConventionOverConfigurationToggle().FeatureEnabled );
		}

		private class ConventionOverConfigurationToggle : SimpleFeatureToggle { }
	}
}
