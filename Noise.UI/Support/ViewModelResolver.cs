using System.Dynamic;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Support {
	internal class DefaultViewModelResolver : IViewModelResolver {
		public object Resolve( string viewModelName ) { return null; }
	}

	public class ViewModelLocator : DynamicObject {
		public ViewModelLocator() {
			Resolver = new DefaultViewModelResolver();
		}

		public IViewModelResolver Resolver { get; set; }

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
		public static IUnityContainer	Container { get; set; }

		public object Resolve( string viewModelName ) {
			object	retValue = null;

			var foundType = GetType().Assembly.GetTypes().FirstOrDefault(type => type.Name == viewModelName );

			if(( foundType != null ) &&
			   ( Container != null )) {
				retValue = Container.Resolve(foundType);
			}

			return( retValue );
		}
	}
}
