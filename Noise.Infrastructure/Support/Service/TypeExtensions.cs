using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Infrastructure.Support.Service {
	public static class TypeExtensions {
		// Queries a type for a list of all attributes of a specific type.
		public static IEnumerable<T> GetAttributes<T>( this Type typeWithAttributes )
			where T : Attribute {
			// Try to find the configuration attribute for the default logger if it exists
			object[] configAttributes = Attribute.GetCustomAttributes( typeWithAttributes, typeof( T ), false );

			// get just the first one
			if( configAttributes != null && configAttributes.Length > 0 ) {
				foreach( T attribute in configAttributes ) {
					yield return attribute;
				}
			}
		}

		// Queries a type for the first attribute of a specific type.
		public static T GetAttribute<T>( this Type typeWithAttributes )
			where T : Attribute {

			return GetAttributes<T>( typeWithAttributes ).FirstOrDefault();
		}
	}
}