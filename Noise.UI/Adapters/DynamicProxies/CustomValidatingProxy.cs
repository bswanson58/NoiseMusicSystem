using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Noise.UI.Adapters.DynamicProxies {
	public class CustomValidatingProxy : ValidatingProxy {
		public class ValidationAttributesCollection {
			private readonly Dictionary<string, List<ValidationAttribute>> mCustomValidationAttributes = new Dictionary<string, List<ValidationAttribute>>();

			public bool HasAttributes( string propertyName ) {
				return
				  mCustomValidationAttributes.ContainsKey( propertyName ) &&
				  mCustomValidationAttributes[propertyName].Count > 0;
			}

			public void Clear( string propertyName ) {
				if( mCustomValidationAttributes.ContainsKey( propertyName )) {
					mCustomValidationAttributes.Remove( propertyName );
				}
			}

			public void Clear() {
				mCustomValidationAttributes.Clear();
			}

			public List<ValidationAttribute> this[string propertyName] {
				get {
					List<ValidationAttribute> returnValue;

					if( !mCustomValidationAttributes.TryGetValue( propertyName, out returnValue )) {
						returnValue = new List<ValidationAttribute>();
						mCustomValidationAttributes.Add( propertyName, returnValue );
					}

					return returnValue;
				}

				set {
					if( mCustomValidationAttributes.ContainsKey( propertyName )) {
						mCustomValidationAttributes.Add( propertyName, value );
					}
					else {
						mCustomValidationAttributes[propertyName] = value;
					}
				}
			}
		}

		public ValidationAttributesCollection ValidationAttributes { get; private set; }

		protected override IEnumerable<ValidationAttribute> GetValidationAttributes( PropertyInfo propertyInfo ) {
			var returnValue = base.GetValidationAttributes( propertyInfo );

			if( ValidationAttributes.HasAttributes( propertyInfo.Name )) {
				returnValue = returnValue.Concat( ValidationAttributes[propertyInfo.Name] );
			}

			return returnValue;
		}

		public CustomValidatingProxy() {
			ValidationAttributes = new ValidationAttributesCollection();
		}

		public CustomValidatingProxy( object proxiedObject )
			: base( proxiedObject ) {
			ValidationAttributes = new ValidationAttributesCollection();
		}
	}
}
