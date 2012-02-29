namespace ReusableBits.Tests.FeatureToggle {
	public class AlwaysOffFeatureToggle : IFeatureToggle {
		public bool FeatureEnabled {
			get { return false; }
		}
	}
}