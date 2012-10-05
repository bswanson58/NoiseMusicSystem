using System;

namespace ReusableBits.Ui.ValueConverters {
	public class TimeFormatter  : BaseValueConverter<DateTime, string> {
		protected override string Convert( DateTime value, System.Globalization.CultureInfo culture ) {
			return( value.ToString( "t" ).ToLower());
		}
	}
}
