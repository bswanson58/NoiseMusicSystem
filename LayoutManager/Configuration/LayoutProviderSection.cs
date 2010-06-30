using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using Composite.Layout.Properties;

namespace Composite.Layout.Configuration {
	public class LayoutProviderSection : IConfigurationSectionHandler {
		#region IConfigurationSectionHandler Members

		public object Create( object parent, object configContext, XmlNode section ) {
			var name = section.Attributes["name"].Value;
			var type = section.Attributes["type"].Value;
			var values = new NameValueCollection();

			foreach( XmlAttribute attribute in section.Attributes ) {
				values.Add( attribute.Name, attribute.Value );
			}

			if( section.ChildNodes.Count > 0 ) {
				values.Add( "InnerXml", section.InnerXml );
			}

			var providerType = Type.GetType( type );
			var layoutProvider = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap( providerType.Assembly.CodeBase, providerType.FullName, null ) as LayoutProviderBase;

			if( layoutProvider == null ) {
				throw new NullReferenceException( Resources.NullLayoutProviderErrorMessage );
			}

			layoutProvider.Initialize( name, values );

			return layoutProvider;
		}

		#endregion
	}
}