using System;

namespace ReusableBits.Tests.FeatureToggle {
	public interface IDateTimeToggleValueProvider {
		DateTime EvaluateDateTimeToggleValue( IFeatureToggle toggle );
	}
}