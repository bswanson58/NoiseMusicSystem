using System.Globalization;
using System.Windows;
using Noise.Infrastructure.Support;

namespace Noise.UI.ValueConverters {
	public class BooleanVisibilityConverter : BaseValueConverter<bool, Visibility> {
		protected override Visibility Convert( bool value, CultureInfo culture ) {
			return value ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
