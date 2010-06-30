using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using Composite.Layout.Extensions;

namespace Composite.Layout.Configuration {
	public abstract class LayoutProviderBase : ProviderBase {
		private readonly NameValueCollection _Configuration = new NameValueCollection();
		public ILayoutManager LayoutManager { get; set; }
		public bool IsInitialized { get; protected set; }

		public new string Name { get; set; }
		public string Type { get; set; }


		public NameValueCollection Configuration {
			get { return _Configuration; }
		}

		public virtual void Initialize( string name ) {
			Initialize( name, new NameValueCollection() );
		}

		public override void Initialize( string name, NameValueCollection config ) {
			if( config == null ) {
				config = new NameValueCollection();
			}

			// Get default name if empty or null
			if( String.IsNullOrEmpty( name ) ) {
				if( String.IsNullOrEmpty( Name ) ) {
					Name = GetType().Name;
				}
			}
			Name = name;


			// Store configuration values
			_Configuration.Clear();

			foreach( string key in config.Keys ) {
				_Configuration.Add( key, config[key] );
			}

			if( _Configuration.ContainsKey( "type" ) ) {
				Type = _Configuration["type"];
			}

			// Call the base class's Initialize method
			base.Initialize( name, config );

			IsInitialized = true;
		}
	}
}