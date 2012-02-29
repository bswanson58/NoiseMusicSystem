using System.Configuration;
using NUnit.Framework;

namespace JasonRoberts.FeatureToggle.Tests {
	[TestFixture]
	public class AppSettingsProviderTests {
		[Test]
		public void ShouldReadBooleanTrueFromConfig() {
			Assert.IsTrue( new AppSettingsProvider().EvaluateBooleanToggleValue( new SimpleFeatureTrue() ) );
		}


		[Test]
		public void ShouldReadBooleanFalseFromConfig() {
			Assert.IsFalse( new AppSettingsProvider().EvaluateBooleanToggleValue( new SimpleFeatureFalse() ) );
		}


		[Test, ExpectedException( typeof( ConfigurationErrorsException ) )]
		public void ShouldErrorWhenKeyNotInConfig() {
			new AppSettingsProvider().EvaluateBooleanToggleValue( new NotInConfig() );
		}


		[Test, ExpectedException( typeof( ConfigurationErrorsException ) )]
		public void ShouldErrorWhenCannotConvertConfig() {
			new AppSettingsProvider().EvaluateBooleanToggleValue( new NotASimpleValue() );
		}


		private class SimpleFeatureTrue : SimpleFeatureToggle { }
		private class SimpleFeatureFalse : SimpleFeatureToggle { }
		private class NotInConfig : SimpleFeatureToggle { }
		private class NotASimpleValue : SimpleFeatureToggle { }
	}
}
