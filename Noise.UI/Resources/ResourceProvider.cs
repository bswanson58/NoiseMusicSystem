using System;
using System.Windows.Media.Imaging;
using Noise.Infrastructure.Interfaces;

namespace Noise.UI.Resources {
	public class ResourceProvider : IResourceProvider {
		public BitmapImage RetrieveImage( string imageName ) {
			var resourceName = string.Format( "{0}{1}", "pack://application:,,,/Noise.UI;component/Resources/", imageName );

			return( new BitmapImage( new Uri( resourceName )));
		}
	}
}
