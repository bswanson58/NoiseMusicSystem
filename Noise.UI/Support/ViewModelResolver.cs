using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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
		[ImportMany( typeof( IBlendableViewModelFactory ))]
		public	List<IBlendableViewModelFactory>	ViewFactories;
		public	static Func<Type,object>			TypeResolver { get; set; }

		static ViewModelResolver() {
			TypeResolver = Activator.CreateInstance;
		}

		public object Resolve( string viewModelName ) {
			object	retValue = null;

			if( Execute.InDesignMode ) {
				retValue = ResolveForDesignMode( viewModelName );
			}

			if(( retValue == null ) &&
			   ( TypeResolver != null )) {
				var viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == viewModelName );

				if( viewModelType != null ) {
					retValue = TypeResolver( viewModelType );
				}
			}

			return( retValue );
		}

		private object ResolveForDesignMode( string viewModelName ) {
			object	retValue = null;

			if( ViewFactories == null ) {
				BuildFactoryCatalog();
			}

			if( ViewFactories != null ) {
				var	viewFactory = ViewFactories.Find( factory => factory.ViewModelType.Name == viewModelName );

				if( viewFactory != null ) {
					retValue = viewFactory.CreateViewModel();
				}
			}

			return( retValue );
		}

		private void BuildFactoryCatalog() {
			var aggCatalog = new AggregateCatalog();
			var container = new CompositionContainer( aggCatalog );
			var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
			var blendAssemblyList = assemblyList.Where( assembly => (( assembly.FullName.Contains( "Blendable" )) &&
																	 (!assembly.FullName.Contains( "Tests" )))).ToList();
			var uniqueList = blendAssemblyList.GroupBy( p => p.FullName ).Select( g => g.First()).ToList();

			uniqueList.ForEach( assembly => aggCatalog.Catalogs.Add( new AssemblyCatalog( assembly )));

			container.ComposeParts( this );
		}
	}
}
