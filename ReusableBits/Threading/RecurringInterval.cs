using System;

namespace ReusableBits.Threading {
	public class RecurringInterval {
		private readonly TimeSpan	mInterval;

		protected RecurringInterval( TimeSpan timeSpan ) {
			mInterval = timeSpan;
		}

		public TimeSpan Interval {
			get{ return( mInterval ); }
		}

		public static RecurringInterval FromMilliseconds( long milliseconds ) {
			return( new RecurringInterval( TimeSpan.FromMilliseconds( milliseconds )));
		}

		public static RecurringInterval FromSeconds( long seconds ) {
			return( new RecurringInterval( TimeSpan.FromSeconds( seconds )));
		}

		public static RecurringInterval FromMinutes( long minutes ) {
			return( new RecurringInterval( TimeSpan.FromMinutes( minutes )));
		}

		public static RecurringInterval FromHours( long hours ) {
			return( new RecurringInterval( TimeSpan.FromHours( hours )));
		}
	}
}
