using System;
using System.Dynamic;
using System.Linq;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.Support {
	public class ViewModelLocator : DynamicObject {
		public static IViewModelResolver	Resolver { get; set; }

		static ViewModelLocator() {
			Resolver = new ViewModelResolver();
		}

		public object this[string viewModelName] {
			get {
				return Resolver.Resolve( viewModelName );
			}
		}

		public override bool TryGetMember( GetMemberBinder binder, out object result ) {
			result = this[binder.Name];

			return( true );
		}
	}

	public class ViewModelResolver : IViewModelResolver {
		public static Func<Type,object>	TypeResolver { get; set; }
	
		static ViewModelResolver() {
			TypeResolver = Activator.CreateInstance;
		}

		public object Resolve( string viewModelName ) {
			object	retValue = null;
			Type	viewModelType = null;

			if( Execute.InDesignMode ) {
				var designModelName = "Blendable" + viewModelName;

				viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == designModelName );
			}

			if( viewModelType == null ) {
				viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == viewModelName );
			}

			if(( viewModelType != null ) &&
			   ( TypeResolver != null )) {
				retValue = TypeResolver( viewModelType );
			}

			return( retValue );
		}
	}
}
