using System.Globalization;
using Noise.Infrastructure.Support;

namespace Noise.UI.ValueConverters {
	public class StringTruncateConverter : BaseValueConverter<string, string> {
		protected override string Convert( string value, CultureInfo culture ) {
			return( !string.IsNullOrWhiteSpace( value ) ? value.Substring( 0, 2 ) : "" );
		}
	}
}
