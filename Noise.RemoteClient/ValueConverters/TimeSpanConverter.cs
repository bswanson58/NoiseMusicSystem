using System;
using System.Globalization;
using Xamarin.Forms;

namespace Noise.RemoteClient.ValueConverters {
	public class TimeSpanConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = String.Empty;

            if( value is Int32 intValue ) {
                retValue = ConvertTimeSpan( TimeSpan.FromMilliseconds( intValue ));
            }
			else if( value is TimeSpan timeSpan ) {
                retValue = ConvertTimeSpan( timeSpan );
			}

			return retValue;
		}

		private string ConvertTimeSpan( TimeSpan timeSpan ) {
            string retValue;

            if( Math.Abs( timeSpan.Hours ) > 0 ) {
                retValue = $"{timeSpan.Hours}:{Math.Abs( timeSpan.Minutes ):D2}:{Math.Abs( timeSpan.Seconds ):D2}";
            }
            else {
                if( Math.Abs( timeSpan.Minutes ) > 0 ) {
                    retValue = $"{timeSpan.Minutes}:{Math.Abs( timeSpan.Seconds ):D2}";
                }
                else {
                    if( timeSpan.Seconds >= 0 ) {
                        retValue = $"0:{Math.Abs( timeSpan.Seconds ):D2}";
                    }
                    else {
                        retValue = $"-0:{Math.Abs( timeSpan.Seconds ):D2}";
                    }
                }
            }

            return retValue;
        }

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			return null;
		}
	}
}
