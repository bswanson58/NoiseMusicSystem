﻿using System;
using System.Globalization;
using System.Windows.Data;
using Noise.Infrastructure;

namespace Noise.UI.ValueConverters {
	public class PublishedYearConverter : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = "";

			if( value is UInt32 ) {
				var year = (UInt32)value;

				if( year == Constants.cUnknownYear ) {
					retValue = "n/a";
				}
				else if( year == Constants.cVariousYears ) {
					retValue = "Various";
				}
				else {
					retValue = string.Format( "{0:D4}", year );
				}
			}
			return( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
