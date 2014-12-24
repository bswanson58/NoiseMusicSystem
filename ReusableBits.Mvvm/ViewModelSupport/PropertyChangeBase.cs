using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace ReusableBits.Mvvm.ViewModelSupport {
	public class PropertyChangeBase : DynamicObject, INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ) );
		}

		protected virtual void RaisePropertyChanged( string propertyName ) {
			Execute.OnUIThread( () => PropertyChanged( this, new PropertyChangedEventArgs( propertyName )));
		}

		protected void RaiseAllPropertiesChanged() {
			RaisePropertyChanged( string.Empty );
		}

		protected static string PropertyName<T>( Expression<Func<T>> expression ) {
			var memberExpression = expression.Body as MemberExpression;

			if( memberExpression == null ) {
				throw new ArgumentException( "expression must be a property expression" );
			}

			return memberExpression.Member.Name;
		}
	}
}
