using System;
using System.Globalization;
using System.Windows.Data;

namespace ReusableBits.Ui.ValueConverters {
	public abstract class BaseValueConverter<TValue, TTarget, TParameter> : IValueConverter {
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( value.GetType() != typeof( TValue )) {
				throw new ArgumentException( GetType().Name + ".Convert: value type not " + typeof( TValue ).Name );
			}
			if( targetType != typeof( TTarget )) {
				throw new ArgumentException( GetType().Name + ".Convert: target type not " + typeof( TTarget ).Name );
			}
			return Convert( (TValue)value, (TParameter)parameter, culture );
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) {
			if( value.GetType() != typeof( TValue )) {
				throw new ArgumentException( GetType().Name + ".ConvertBack: value type not " + typeof( TValue ).Name );
			}
			if( targetType != typeof( TTarget )) {
				throw new ArgumentException( GetType().Name + ".ConvertBack: target type not " + typeof( TTarget ).Name );
			}
			return ConvertBack( (TValue)value, parameter, culture );
		}

		protected virtual TTarget Convert( TValue value, TParameter parameter, CultureInfo culture ) {
			throw new NotImplementedException( GetType().Name + ".Convert not implemented" );
		}

		protected virtual TTarget ConvertBack( TValue value, object parameter, CultureInfo culture ) {
			throw new NotImplementedException( GetType().Name + ".ConvertBack not implemented" );
		}
	}

	public abstract class BaseValueConverter<TValue, TTarget>
		: BaseValueConverter<TValue, TTarget, object> {
		protected virtual TTarget Convert( TValue value, CultureInfo culture ) {
			throw new NotImplementedException( GetType().Name + ".Convert not implemented" );
		}

		protected virtual TTarget ConvertBack( TValue value, CultureInfo culture ) {
			throw new NotImplementedException( GetType().Name + ".ConvertBack not implemented" );
		}

		protected sealed override TTarget Convert( TValue value, object parameter, CultureInfo culture ) {
			if( parameter != null ) {
				throw new ArgumentException( GetType().Name + ".Convert: binding contains unexpected parameter" );
			}
			return Convert( value, culture );
		}

		protected sealed override TTarget ConvertBack( TValue value, object parameter, CultureInfo culture ) {
			if( parameter != null ) {
				throw new ArgumentException( GetType().Name + ".ConvertBack: binding contains unexpected parameter" );
			}
			return ConvertBack( value, culture );
		}
	}
}
