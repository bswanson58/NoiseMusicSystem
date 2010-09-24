using System;
using CuttingEdge.Conditions;

namespace Noise.UI.Adapters.DynamicProxies {
	public class UpdatingProxy : NotifyingProxy {
		public UpdatingProxy() { }

		public UpdatingProxy( object proxiedObject ) :
			base( proxiedObject ) {
		}

		public void UpdateObject( object newObject ) {
			Condition.Requires( newObject ).IsNotNull();

			if( newObject.GetType() == ProxiedObject.GetType() ) {
				var members = ProxiedObject.GetType().GetProperties();

				foreach( var member in members ) {
					if( member.CanWrite ) { 
						SetMember( member.Name, member.GetValue( newObject, null ));
					}
				}

			}
			else {
				throw new ApplicationException( "Updating proxy types are incompatible" );
			}
		}
	}
}
