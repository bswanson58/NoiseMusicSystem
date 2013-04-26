using System.Globalization;
using ReusableBits.Ui.ValueConverters;

namespace Noise.UI.ValueConverters {
	public class StringTruncateConverter : BaseValueConverter<string, string> {
		protected override string Convert( string value, CultureInfo culture ) {
			return(!string.IsNullOrWhiteSpace( value ) ? value.Length > 1 ? value.Substring( 0, 2 ) : value : string.Empty );
		}
	}
}
