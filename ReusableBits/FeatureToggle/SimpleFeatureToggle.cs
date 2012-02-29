#if (WINDOWS_PHONE)
    using JasonRoberts.FeatureToggle.Wp7;
#endif


namespace ReusableBits.Tests.FeatureToggle {
	public abstract class SimpleFeatureToggle : IFeatureToggle {
		protected SimpleFeatureToggle() {
#if (WINDOWS_PHONE)

            BooleanToggleValueProvider = new WindowsPhone7ApplicationResourcesSettingsProvider();
#else

			BooleanToggleValueProvider = new AppSettingsProvider();
#endif
		}


		public IBooleanToggleValueProvider BooleanToggleValueProvider { get; set; }


		public bool FeatureEnabled {
			get { return BooleanToggleValueProvider.EvaluateBooleanToggleValue( this ); }
		}

		public override string ToString() {
			return GetType().Name;
		}
	}
}