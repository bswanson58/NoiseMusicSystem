using System;

namespace ReusableBits.Tests.FeatureToggle {
	public interface INowDateAndTime {
		DateTime Now { get; }
	}
}
