using System.Globalization;
using System.Windows;

namespace ReusableBits.Ui.ValueConverters {
	public class BooleanVisibilityConverter : BaseValueConverter<bool, Visibility> {
		protected override Visibility Convert( bool value, CultureInfo culture ) {
			return value ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
