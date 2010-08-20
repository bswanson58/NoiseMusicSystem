using System;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Dynamic;

namespace Noise.UI.Adapters.DynamicProxies {
	public class DataErrorInfoProxy : ValidatingProxy, IDataErrorInfo {
		protected override bool Validate( PropertyInfo propertyInfo, object value ) {
			var returnValue = base.Validate( propertyInfo, value );
			return returnValue;
		}

		public DataErrorInfoProxy() { }
		public DataErrorInfoProxy( object proxiedObject ) : base( proxiedObject ) { }

		public override bool TryConvert( ConvertBinder binder, out object result ) {
			if( binder.Type == typeof( IDataErrorInfo )) {
				result = this;
				return true;
			}

			return base.TryConvert( binder, out result );
		}

		public string Error {
			get {
				var returnValue = new StringBuilder();

				foreach( var item in ValidationResults ) {
					foreach( var validationResult in item.Value ) {
						returnValue.AppendLine( validationResult.ErrorMessage );
					}
				}

				return returnValue.ToString();
			}
		}

		public string this[string columnName] {
			get {
				return ValidationResults.ContainsKey( columnName ) ?
				  string.Join( Environment.NewLine, ValidationResults[columnName] ) :
				  string.Empty;
			}
		}
	}
}
