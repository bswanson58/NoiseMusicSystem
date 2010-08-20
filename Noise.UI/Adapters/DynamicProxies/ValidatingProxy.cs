using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Noise.UI.Adapters.DynamicProxies {
	public class ValidatingProxy : EditableProxy {
		public bool		ValidateOnChange { get; set; }
		public bool		HasErrors { get { return ValidationResults.Count > 0; } }

		protected readonly Dictionary<string, Collection<ValidationResult>> ValidationResults = new Dictionary<string, Collection<ValidationResult>>();

		public ValidatingProxy() {
			ValidateOnChange = true;
		}

		public ValidatingProxy( object proxiedObject )
			: base( proxiedObject ) {
			ValidateOnChange = true;
		}

		protected override void SetMember( string propertyName, object value ) {
			if( ValidateOnChange ) {
				Validate( propertyName, value );
			}

			base.SetMember( propertyName, value );
		}

		protected virtual IEnumerable<ValidationAttribute> GetValidationAttributes( PropertyInfo propertyInfo ) {
			var validationAttributes = new List<ValidationAttribute>();

			foreach( ValidationAttribute item in propertyInfo.GetCustomAttributes( typeof( ValidationAttribute ), true )) {
				validationAttributes.Add( item );
			}

			return validationAttributes;
		}

		protected virtual bool Validate( PropertyInfo propertyInfo, object value ) {
			var validationAttributes = GetValidationAttributes( propertyInfo );
			if( validationAttributes.Count() == 0 ) {
				return true;
			}

			var validationContext = new ValidationContext( ProxiedObject, null, null );
			var validationResults = new Collection<ValidationResult>();

			var returnValue = Validator.TryValidateValue(
			  value,
			  validationContext,
			  validationResults,
			  validationAttributes );

			if( returnValue ) {
				if( ValidationResults.ContainsKey( propertyInfo.Name )) {
					ValidationResults.Remove( propertyInfo.Name );
				}
			}
			else {
				if( ValidationResults.ContainsKey( propertyInfo.Name )) {
					ValidationResults[propertyInfo.Name] = validationResults;
				}
				else {
					ValidationResults.Add( propertyInfo.Name, validationResults );
				}
			}

			return returnValue;
		}

		protected virtual bool Validate( string propertyName, object value ) {
			return Validate( GetPropertyInfo( propertyName ), value );
		}

		public virtual bool Validate( PropertyInfo propertyInfo ) {
			return Validate( propertyInfo, GetMember( propertyInfo.Name ));
		}

		public virtual bool Validate( string propertyName ) {
			return Validate( GetPropertyInfo( propertyName ));
		}

		public virtual bool Validate() {
			var propertiesToValidate = ProxiedObject.GetType().GetProperties().Where(
			  pi => pi.GetCustomAttributes( typeof( ValidationAttribute ), true ).Length > 0 );

			var returnValue = false;
			foreach( var item in propertiesToValidate ) {
				returnValue |= Validate( item );
			}

			return returnValue;
		}
	}
}
