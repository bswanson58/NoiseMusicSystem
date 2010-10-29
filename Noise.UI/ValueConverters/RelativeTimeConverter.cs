using System;
using System.Globalization;
using System.Windows.Data;

// from: http://www.blackwasp.co.uk/RelativeTime.aspx

namespace Noise.UI.ValueConverters {
	public class RelativeTimeConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is DateTime ) {
				var date = (DateTime)value;

				retValue = GetRelativeTime( date );
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}

		private static string GetRelativeTime( DateTime time ) {
			var retValue = "Just Now";
			var difference = DateTime.UtcNow - time.ToUniversalTime();
			var positive = TimeSpan.FromSeconds((int)Math.Abs( difference.TotalSeconds ) );

			if( positive.TotalMinutes > 0 ) {
				if( positive.TotalDays < 4 ) {
					string relativeTime = GetOffset( positive );
					string suffix = GetSuffix( difference );

					retValue = string.Format( "{0} {1}", relativeTime, suffix );
				}
				else {
					retValue = time.ToShortDateString();
				}
			}

			return( retValue );
		}

		private static string GetOffset( TimeSpan positive ) {
			if( positive.Days >= 365 )
				return GetOffsetWords( positive.Days / 365, "Year" );
			if( positive.Days >= 7 )
				return GetOffsetWords( positive.Days / 7, "Week" );
			if( positive.Days >= 1 )
				return GetOffsetWords( positive.Days, "Day" );
			if( positive.Hours >= 1 )
				return GetOffsetWords( positive.Hours, "Hour" );
			if( positive.Minutes >= 1 )
				return GetOffsetWords( positive.Minutes, "Minute" );
			
			return GetOffsetWords( positive.Seconds, "Second" );
		}

		private static string GetOffsetWords( int offset, string unit ) {
			string offsetWords = new NumberToWordsConverter().NumberToWords( offset );
			if( offset != 1 ) unit += "s";
			return string.Format( "{0} {1}", offsetWords, unit );
		}

		private static string GetSuffix( TimeSpan difference ) {
			return difference.TotalSeconds > 0 ? "Ago" : "in the Future";
		}
	}
}
