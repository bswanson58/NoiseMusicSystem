using System.Windows.Media.Imaging;

namespace Noise.Infrastructure.Interfaces {
	public interface IResourceProvider {
		BitmapImage		RetrieveImage( string imageName );
		object			RetrieveTemplate( string templateName );
	}
}
