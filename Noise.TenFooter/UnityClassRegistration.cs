using System;
using System.Linq;
using Noise.TenFoot.Ui.Interfaces;
using UnityConfiguration;

namespace Noise.TenFooter {
	public class UnityClassRegistration : UnityRegistry {
		public UnityClassRegistration() {
			Scan( scan => {
				scan.AssemblyContaining<IHome>();
				scan.With<InterfaceNamespaceConvention>().IncludeInterfaceNamespace( "Noise.TenFoot.Ui.Interfaces" );
			} );
		}

		public class InterfaceNamespaceConvention : IAssemblyScannerConvention {
			private string	mInterfaceNamespace;

			public InterfaceNamespaceConvention() {
				mInterfaceNamespace = "";
			}

			public InterfaceNamespaceConvention IncludeInterfaceNamespace( string interfaceNamespace ) {
				mInterfaceNamespace = interfaceNamespace;

				return( this );
			}

			void IAssemblyScannerConvention.Process( Type type, IUnityRegistry registry ) {
				if((!type.IsConcrete()) ||
				   (!type.CanBeCreated())) {
					return;
				}

				Type interfaceType = SelectInterfaceType( type );

				if( interfaceType != null ) {
					registry.Register( interfaceType, type );
				}
			}

			private Type SelectInterfaceType( Type type ) {
				Type[]	interfaces = type.GetInterfaces();
				Type	interfaceType = interfaces.FirstOrDefault( i => (!string.IsNullOrWhiteSpace( i.Namespace )) && 
																		( i.Namespace.StartsWith( mInterfaceNamespace )));

				return interfaceType;
			}
		}
	}
}
