using Moq;
using NUnit.Framework;

namespace ReusableBits.Tests.FeatureToggle {
	[TestFixture]
	public class SimpleFeatureToggleTests {
		[Test]
		public void ShouldSetOptionalProviderOnCreation() {
			var fakeProvider = new Mock<IBooleanToggleValueProvider>();

			fakeProvider.Setup( x => x.EvaluateBooleanToggleValue( It.IsAny<SimpleFeatureToggle>() ) ).Returns( true );

			var sut = new MySimpleFeatureToggle();
			sut.BooleanToggleValueProvider = fakeProvider.Object;

			Assert.AreEqual( true, sut.FeatureEnabled );
		}

		private class MySimpleFeatureToggle : SimpleFeatureToggle { }
	}
}
