using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace ReusableBits.Mvvm.ViewModelSupport {
	public class AutomaticPropertyBase : DependantPropertyBase {
		private readonly Dictionary<string, object>			mValues;

		protected AutomaticPropertyBase() {
			mValues = new Dictionary<string, object>();
		}

		protected T Get<T>( string name ) {
			return Get( name, default( T ));
		}

		protected T Get<T>( string name, T defaultValue ) {
			if( mValues.ContainsKey( name )) {
				return (T)mValues[name];
			}

			return defaultValue;
		}

		protected T Get<T>( string name, Func<T> initialValue ) {
			if( mValues.ContainsKey( name )) {
				return (T)mValues[name];
			}

			Set( name, initialValue() );
			return Get<T>( name );
		}

		protected T Get<T>( Expression<Func<T>> expression ) {
			return Get<T>( PropertyName( expression ) );
		}

		protected T Get<T>( Expression<Func<T>> expression, T defaultValue ) {
			return Get( PropertyName( expression ), defaultValue );
		}

		protected T Get<T>( Expression<Func<T>> expression, Func<T> initialValue ) {
			return Get( PropertyName( expression ), initialValue );
		}

		public void Set<T>( string name, T value ) {
			if( mValues.ContainsKey( name ) ) {
				if(( mValues[name] == null ) &&
				   ( Equals( value, default( T )))) {
					return;
				}

				if(( mValues[name] != null ) &&
				   ( mValues[name].Equals( value ))) {
					return;
				}

				mValues[name] = value;
			}
			else {
				mValues.Add( name, value );
			}

			RaisePropertyChanged( name );
		}

		protected void Set<T>( Expression<Func<T>> expression, T value ) {
			Set( PropertyName( expression ), value );
		}

		public override bool TryGetMember( GetMemberBinder binder, out object result ) {
			result = Get<object>( binder.Name );

			if( result != null ) {
				return true;
			}

			return base.TryGetMember( binder, out result );
		}

		public override bool TrySetMember( SetMemberBinder binder, object value ) {
			var result = base.TrySetMember( binder, value );
			if( result ) {
				return true;
			}

			Set( binder.Name, value );

			return true;
		}
	}
}
