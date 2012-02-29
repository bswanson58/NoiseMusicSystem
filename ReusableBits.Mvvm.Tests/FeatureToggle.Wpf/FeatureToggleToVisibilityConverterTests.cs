using System.Windows;
using ReusableBits.Mvvm.FeatureToggle.Wpf;
using ReusableBits.Tests.FeatureToggle;
using Moq;
using NUnit.Framework;

namespace ReusableBits.Mvvm.Tests.FeatureToggle.Wpf {
	[TestFixture]
	public class FeatureToggleToVisibilityConverterTests {
		[Test]
		public void ShouldCovertTrueToVisisble() {
			var mockValueProvider = new Mock<IBooleanToggleValueProvider>();

			mockValueProvider.Setup( x => x.EvaluateBooleanToggleValue( It.IsAny<SimpleFeatureToggle>() ) ).Returns( true );

			var toggle = new MyBooleanFeatureToggle {
				BooleanToggleValueProvider = mockValueProvider.Object
			};

			var sut = new FeatureToggleToVisibilityConverter();

			var result = sut.Convert( toggle, typeof( Visibility ), null, null );

			Assert.AreEqual( Visibility.Visible, result );
		}

		private class MyBooleanFeatureToggle : SimpleFeatureToggle { }
	}
}
