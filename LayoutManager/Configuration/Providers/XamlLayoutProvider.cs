using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Markup;
using Composite.Layout.Extensions;
using Composite.Layout.Properties;

namespace Composite.Layout.Configuration {
	public class XamlLayoutProvider : LayoutProviderBase {
		public string ConfigType { get; set; }
		public string Filename { get; set; }

		public override void Initialize( string name, NameValueCollection config ) {
			base.Initialize( name, config );

			//load LayoutManager by type
			if( Configuration.ContainsKey( "configType" ) ) {
				ConfigType = Configuration["configType"];

				if( !string.IsNullOrEmpty( ConfigType ) ) {
					var type = System.Type.GetType( ConfigType );
					LayoutManager = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap( type.Assembly.CodeBase, type.FullName, null ) as LayoutManager;
				}
			}

			//load loose Xaml
			if( LayoutManager == null && Configuration.ContainsKey( "filename" ) ) {
				Filename = Configuration["filename"];

				string path = null;

				if( File.Exists( Filename ) ) {
					path = Filename;
				}
				else if( File.Exists( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, Filename ) ) ) {
					path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, Filename );
				}

				if( !string.IsNullOrEmpty( path ) ) {
					var stream = File.Open( path, FileMode.Open, FileAccess.Read );
					LayoutManager = XamlReader.Load( stream ) as ILayoutManager;
					Filename = path;
				}
			}

			if( LayoutManager == null ) {
				throw new NullReferenceException( Resources.NullLayoutManagerErrorMessage );
			}

			IsInitialized = true;
		}
	}
}