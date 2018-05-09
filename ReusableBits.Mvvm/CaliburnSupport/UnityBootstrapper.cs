using System;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;

namespace ReusableBits.Mvvm.CaliburnSupport {
	public class UnityBootstrapper<TRootViewModel> : BootstrapperBase {
		protected IUnityContainer Container { get; private set; }

        public UnityBootstrapper() {
            Initialize();
        }

		/// <summary>
		/// Method for creating the window manager
		/// </summary>
		public Func<IWindowManager> CreateWindowManager { get; set; }
		/// <summary>
		/// Method for creating the event aggregator
		/// </summary>
		public Func<IEventAggregator> CreateEventAggregator { get; set; }

		protected override void Configure() {
		    // Caliburn Micro dispatcher initialize.
		    PlatformProvider.Current = new XamlPlatformProvider();

			// allow base classes to change bootstrapper settings
			ConfigureBootstrapper();

			// Creat the container
			Container = new UnityContainer();

			// register the single window manager for this container
			if( CreateWindowManager != null ) {
				Container.RegisterInstance( CreateWindowManager(), new ContainerControlledLifetimeManager());
			}

			// register the single event aggregator for this container
			if( CreateEventAggregator != null ) {
				Container.RegisterInstance( CreateEventAggregator(), new ContainerControlledLifetimeManager());
			}

			// Allow derived classes to add to the container
			ConfigureContainer( Container );
		}

	    protected override void OnStartup( object sender, StartupEventArgs e ) {
	        base.OnStartup( sender, e );

            DisplayRootViewFor<TRootViewModel>();
	    }

	    /// <summary>
		/// Do not override unless you plan to full replace the logic. This is how the framework
		/// retrieves services from the Unity container.
		/// </summary>
		/// <param name="service">The service to locate.</param>
		/// <param name="key">The key to locate.</param>
		/// <returns>The located service.</returns>
		protected override object GetInstance( Type service, string key ) {
			object instance = string.IsNullOrWhiteSpace( key ) ? Container.Resolve( service ) :
																 Container.Resolve( service, key );

			if( instance == null ) {
				throw new Exception( string.Format( "Could not locate any instances of contract {0}.", key ?? service.Name ) );
			}

			return ( instance );
		}

		/// <summary>
		/// Do not override unless you plan to full replace the logic. This is how the framework
		/// retrieves services from the Unity container.
		/// </summary>
		/// <param name="service">The service to locate.</param>
		/// <returns>The located services.</returns>
		protected override System.Collections.Generic.IEnumerable<object> GetAllInstances( Type service ) {
			return ( Container.ResolveAll( service ) );
		}

		/// <summary>
		/// Do not override unless you plan to full replace the logic. This is how the framework
		/// retrieves services from the Unity container.
		/// </summary>
		/// <param name="instance">The instance to perform injection on.</param>
		protected override void BuildUp( object instance ) {
			Container.BuildUp( instance );
		}

		/// <summary>
		/// Override to provide configuration prior to the Unity container configuration.
		/// Current Defaults:
		/// 
		/// CreateWindowManager = <see cref="Caliburn.Micro.WindowManager"/>
		/// CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator"/>
		/// </summary>
		protected virtual void ConfigureBootstrapper() {
			// The default window manager
			CreateWindowManager = () => new WindowManager();
			// The default event aggregator
			CreateEventAggregator = () => new EventAggregator();
		}

		/// <summary>
		/// Override to include your own Unity configuration after the framework has finished its configuration, but
		/// before the container is used to resolve any instances.
		/// </summary>
		/// <param name="container">The Unity container.</param>
		protected virtual void ConfigureContainer( IUnityContainer container ) {
		}

		protected void AddModule( Type moduleType ) {
			var module = Container.Resolve( moduleType ) as IModule;

		    module?.Initialize();
		}
	}
}
