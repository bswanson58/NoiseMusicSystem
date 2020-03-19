using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

// from: https://thomasfreudenberg.com/archive/2017/01/21/presenting-byte-size-values-in-wpf/

namespace ReusableBits.Ui.ValueConverters {
	public class NumericTextMultitudeConverter : IValueConverter {
/*
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern long StrFormatByteSizeW(long qdw, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf, int cchBuf);

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var number = System.Convert.ToInt64( value );
            var sb = new StringBuilder( 32 );
            
            StrFormatByteSizeW( number, sb, sb.Capacity );

            return sb.ToString();
        }
*/
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var lValue = System.Convert.ToInt64( value );
			var formatValue = (double)lValue;
			var suffix = String.Empty;

            if( Math.Abs( lValue ) > 999999999 ) {
                suffix = "G";

                formatValue = lValue / 1000000000.0;
            }
			else if( Math.Abs( lValue ) > 999999 ) {
				suffix = "M";

				formatValue = lValue / 1000000.0;
			}
            else if( Math.Abs( lValue ) > 999 ) {
                suffix = "K";

                formatValue = lValue / 1000.0;
            }
            else {
                return (int)formatValue;
            }

			return( $"{formatValue:N2}{suffix}" );
		}

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            return DependencyProperty.UnsetValue;
        }
	}
}
