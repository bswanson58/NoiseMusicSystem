using System;

namespace ReusableBits.Support {
	public static class TimeProvider {
		public static Func<DateTime> Now;

		static TimeProvider() {
			Reset();
		}

		public static void Reset() {
			Now = () => DateTime.Now;
		}
	}
}
