using System;

namespace ReusableBits.Tests.FeatureToggle {
	public class NowDateAndTime : INowDateAndTime {
		public DateTime Now {
			get { return DateTime.Now; }
		}
	}
}
