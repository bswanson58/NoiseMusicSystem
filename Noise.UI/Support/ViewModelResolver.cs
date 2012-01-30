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

		private void BuildCatalog() {
			var catalog = new DirectoryCatalog(  @".\" );
			var container = new CompositionContainer( catalog );

			container.ComposeParts( this );
		}
	
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
				BuildCatalog();
			}

			if( ViewFactories != null ) {
				var	designModelName = "Blendable" + viewModelName;
				var	viewFactory = ViewFactories.Find( factory => factory.ViewModelType.Name == designModelName );

				if( viewFactory != null ) {
					retValue = viewFactory.CreateViewModel();
				}

//				var		viewModelType = GetType().Assembly.GetTypes().FirstOrDefault( type => type.Name == designModelName );
//				if( viewModelType != null ) {
//					var creator = Activator.CreateInstance( viewModelType ) as IBlendableViewModelFactory;
//
//					if( creator != null ) {
//						retValue = creator.CreateViewModel();
//					}
//				}
			}

			return( retValue );
		}
	}
}
