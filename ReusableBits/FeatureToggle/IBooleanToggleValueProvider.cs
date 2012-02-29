namespace ReusableBits.Tests.FeatureToggle {
	public interface IBooleanToggleValueProvider {
		bool EvaluateBooleanToggleValue( IFeatureToggle toggle );
	}
}
