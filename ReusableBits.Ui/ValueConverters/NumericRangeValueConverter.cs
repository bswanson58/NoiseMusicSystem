using System;
using System.Globalization;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	/// <summary>
	/// This value converter will translate an input value within a stated input range to
	/// a comparable value within a given output range.
	/// For instance:
	/// <ns:NumericRangeValueConverter x:Key="RangeConverter" MinimumInput="0" MaximumInput="100" MinimumOutput="0.2" MaximumOutput="1.0"/>
	/// Will translate an input value between 0 and 100 to an output value ranging from 0.2 to 1.0
	/// 
	/// </summary>
	public class NumericRangeValueConverter : IValueConverter {
		public double MaximumInput { get; set; }
		public double MinimumInput { get; set; }
		public double MaximumOutput { get; set; }
		public double MinimumOutput { get; set; }

		public NumericRangeValueConverter() {
			MaximumInput = 1.0D;
			MinimumInput = 0.0D;
			MaximumOutput = 1.0D;
			MinimumOutput = 0.0D;
		}

		private double ClampValues( double inputValue ) {
			if( MinimumInput >= MaximumInput ) {
				MinimumInput = MaximumInput;
			}

			if( inputValue < MinimumInput ) {
				inputValue = MinimumInput;
			}
			if( inputValue > MaximumInput ) {
				inputValue = MaximumInput;
			}

			return( inputValue );
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			var retValue = MinimumOutput;

			if( value != null ) {
				try {
					var numericValue = ClampValues( System.Convert.ToDouble( value ));

					var inputOffset = ( numericValue - MinimumInput ) / ( MaximumInput - MinimumInput );
					var outputOffset = ( MaximumOutput - MinimumOutput ) * inputOffset;

					retValue = MinimumOutput + outputOffset;
				}
				catch( Exception ) {
					retValue = MaximumOutput;
				}
			}

			return ( retValue );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
