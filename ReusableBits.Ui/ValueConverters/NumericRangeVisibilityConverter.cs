using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
    public class NumericRangeVisibilityConverter : IValueConverter {
        // The numerical upper and lower limits to be matched inclusive, defaults to zero.
        public long LowerLimit { get; set; }
        public long UpperLimit { get; set; }

        // Set to true if you just want to hide the control
        // else set to false if you want to collapse the control
        public bool IsHidden { get; set; }

        // Set to true if you want visibility when the numerical value matches,
        // or set to false if you want visibility when the values are not equal.
        public bool HideOnMatch { get; set; }

        public NumericRangeVisibilityConverter() {
            LowerLimit = 0;
            UpperLimit = 0;
            IsHidden = true;
            HideOnMatch = true;
        }

        public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
            var retValue = Visibility.Visible;

            if( value != null ) {
                var intValue = System.Convert.ToInt64( value );

                if(( intValue >= LowerLimit ) &&
                   ( intValue <= UpperLimit )) {
                    if( HideOnMatch ) {
                        retValue = IsHidden ? Visibility.Hidden : Visibility.Collapsed;
                    }
                }
                else {
                    if(!HideOnMatch ) {
                        retValue = IsHidden ? Visibility.Hidden : Visibility.Collapsed;
                    }
                }
            }

            return ( retValue );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
            throw new NotImplementedException();
        }
    }
}
