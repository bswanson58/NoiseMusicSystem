using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class HtmlTextConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is string ) {
				var input = (string)value;
				var regEx = new Regex( @"<(.|\n)*?>", RegexOptions.IgnoreCase );

				retValue = regEx.Replace( input, String.Empty );
				retValue = retValue.Replace( "&quot;", "'" );
				retValue = retValue.Replace( "&amp;", "&" );
				retValue = retValue.Replace( "&lt;", "<" );
				retValue = retValue.Replace( "&gt;", ">" );

				var len = 9600;

				if( retValue.Length > len ) {
					retValue = retValue.Substring( 0, len ) + " (truncated!)";
				}
			}

			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
