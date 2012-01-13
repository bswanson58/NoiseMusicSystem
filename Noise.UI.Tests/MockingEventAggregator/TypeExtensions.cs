using System;
using System.Reflection;

namespace Noise.UI.Tests.MockingEventAggregator {
	public static class TypeExtensions {

		public static FieldInfo GetPrivateField( this Type type, string fieldName ) {
			return type.GetField( fieldName, BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic )
				   ?? type.BaseType.GetPrivateField( fieldName );
		}
	}
}