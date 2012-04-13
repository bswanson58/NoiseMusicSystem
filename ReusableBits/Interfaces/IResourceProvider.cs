using System.Windows.Media.Imaging;

namespace ReusableBits.Interfaces {
	public interface IResourceProvider {
		BitmapImage		RetrieveImage( string imageName );
		object			RetrieveTemplate( string templateName );
	}
}
