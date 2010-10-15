using System;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class TimeSpanConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is TimeSpan ) {
				var timeSpan = (TimeSpan)value;

				if( timeSpan.TotalSeconds > 1 ) {
					if( Math.Abs( timeSpan.Hours ) > 0 ) {
						retValue = string.Format( "{0}:{1:D2}:{2:D2}", timeSpan.Hours, Math.Abs( timeSpan.Minutes ), Math.Abs( timeSpan.Seconds ));
					}
					else {
						if( Math.Abs( timeSpan.Minutes ) > 0 ) {
							retValue = string.Format( "{0}:{1:D2}", timeSpan.Minutes, Math.Abs( timeSpan.Seconds ));
						}
						else {
							if( timeSpan.Seconds > 0 ) {
								retValue = string.Format(  "0:{0:D2}", Math.Abs( timeSpan.Seconds ));
							}
							else {
								retValue = string.Format(  "-0:{0:D2}", Math.Abs( timeSpan.Seconds ));
							}
						}
					}
				}
			}
			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return( null );
		}
	}
}
