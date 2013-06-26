using System.Xml;
using ReusableBits.Configuration;

namespace Noise.Infrastructure.Configuration {
	public class ConfigurationUpgrades : BaseConfigurationUpgrader {
		// ReSharper disable InconsistentNaming
		private static void UpgradeFrom_0_9_0_0( XmlElement configuration ) {
			var explorerConfiguration = configuration.SelectNodes( ExplorerConfiguration.SectionName );

			if( explorerConfiguration != null ) {
				var explorerNode = explorerConfiguration[0];

				if(( explorerNode != null ) &&
				   ( explorerNode.Attributes != null )) {
					var attr = explorerNode.Attributes["playExhaustedItem"];

					if( attr != null ) {
						explorerNode.Attributes.Remove( attr );
					}
				}
			}
		}
		// ReSharper restore InconsistentNaming
	}
}
