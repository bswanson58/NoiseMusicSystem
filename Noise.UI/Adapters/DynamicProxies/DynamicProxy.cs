using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.ComponentModel;
using System;

// from: http://www.codeproject.com/KB/cs/dynamicobjectproxy.aspx

namespace Noise.UI.Adapters.DynamicProxies {
	public class DynamicProxy : DynamicObject, INotifyPropertyChanged {
		public object	ProxiedObject { get; set; }
		public event	PropertyChangedEventHandler PropertyChanged;

		public DynamicProxy() { }

		public DynamicProxy( object proxiedObject ) {
			ProxiedObject = proxiedObject;
		}

		protected PropertyInfo GetPropertyInfo( string propertyName ) {
			return ProxiedObject.GetType().GetProperties().First( propertyInfo => propertyInfo.Name == propertyName );
		}

		protected virtual void SetMember( string propertyName, object value ) {
			var propertyInfo = GetPropertyInfo( propertyName );

			if( propertyInfo.PropertyType == value.GetType()) {
				propertyInfo.SetValue( ProxiedObject, value, null );
			}
			else {
				propertyInfo.SetValue( ProxiedObject, Convert.ChangeType( value, propertyInfo.PropertyType ), null );
			}

			RaisePropertyChanged( propertyName );
		}

		protected virtual object GetMember( string propertyName ) {
			return GetPropertyInfo( propertyName ).GetValue( ProxiedObject, null );
		}

		protected virtual void OnPropertyChanged( string propertyName ) {
			if( PropertyChanged != null ) {
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ));
			}
		}

		protected virtual void RaisePropertyChanged( string propertyName ) {
			OnPropertyChanged( propertyName );
		}

		public override bool TryConvert( ConvertBinder binder, out object result ) {
			if( binder.Type == typeof( INotifyPropertyChanged )) {
				result = this;
				return true;
			}

			if( ProxiedObject != null && binder.Type.IsAssignableFrom( ProxiedObject.GetType() ) ) {
				result = ProxiedObject;
				return true;
			}

			return base.TryConvert( binder, out result );
		}

		public override bool TryGetMember( GetMemberBinder binder, out object result ) {
			result = GetMember( binder.Name );

			return true;
		}

		public override bool TrySetMember( SetMemberBinder binder, object value ) {
			SetMember( binder.Name, value );

			return true;
		}

	}
}
