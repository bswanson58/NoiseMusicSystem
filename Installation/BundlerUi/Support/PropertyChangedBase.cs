using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace BundlerUi.Support {
	public class PropertyChangedBase : INotifyPropertyChanged {
		private Dispatcher	mDispatcher;

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void SetNotificationDispatcher( Dispatcher dispatcher ) {
			mDispatcher = dispatcher;
		}

		protected void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ));
		}

		protected virtual void RaisePropertyChanged( string propertyName ) {
			if( mDispatcher != null ) {
				mDispatcher.Invoke( () => PropertyChanged( this, new PropertyChangedEventArgs( propertyName )));
			}
			else {
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ));
			}
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
