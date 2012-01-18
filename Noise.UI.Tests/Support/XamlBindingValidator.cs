using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using NUnit.Framework;

namespace Noise.UI.Tests.Support {
	public class XamlBindingValidator {
		private delegate void FoundBindingCallbackDelegate( FrameworkElement element, Binding binding, DependencyProperty dp );

		public void CheckWpfBindingsAreValid( FrameworkElement view, Type viewModelType ) {

			FindBindingsRecursively( view,
					delegate( FrameworkElement element, Binding binding, DependencyProperty dp ) {
						// check that each part of binding valid via reflection
						foreach( string prop in binding.Path.Path.Split( '.' ) ) {
							PropertyInfo info = viewModelType.GetProperty( prop );
							Assert.IsNotNull( info, string.Format( "Cannot locate binding for '{0}'", binding.Path.Path ));
//							type = info.PropertyType;
						}
					} );
		}

		private void FindBindingsRecursively( DependencyObject element, FoundBindingCallbackDelegate callbackDelegate ) {
			// See if we should display the errors on this element
			MemberInfo[] members = element.GetType().GetMembers( BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy );

			foreach( MemberInfo member in members ) {
				DependencyProperty dp = null;

				// Check to see if the field or property we were given is a dependency property
				if( member.MemberType == MemberTypes.Field ) {
					var field = (FieldInfo)member;

					if( typeof( DependencyProperty ).IsAssignableFrom( field.FieldType )) {
						dp = (DependencyProperty)field.GetValue( element );
					}
				}
				else if( member.MemberType == MemberTypes.Property ) {
					var prop = (PropertyInfo)member;

					if( typeof( DependencyProperty ).IsAssignableFrom( prop.PropertyType )) {
						dp = (DependencyProperty)prop.GetValue( element, null );
					}
				}

				if( dp != null ) {
					// We have a dependency property. does it have a binding? If yes, is it bound to the property we're interested in?
					Binding bb = BindingOperations.GetBinding( element, dp );

					if( bb != null ) {
						// This element has a DependencyProperty that we know of that is bound to the property we're interested in. 
						// Now we just tell the callback and the caller will handle it.
						var frameworkElement = element as FrameworkElement;

						if( frameworkElement != null ) {
							callbackDelegate( frameworkElement, bb, dp );
						}
					}
				}
			}

			// Now, recurse through any child elements
			if(( element is FrameworkElement ) ||
			   ( element is FrameworkContentElement )) {
				foreach( object childElement in LogicalTreeHelper.GetChildren( element )) {
					var dependencyObject = childElement as DependencyObject;

					if( dependencyObject != null ) {
						FindBindingsRecursively( dependencyObject, callbackDelegate );
					}
				}
			}
		}
	}
}
