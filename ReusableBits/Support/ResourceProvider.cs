using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ReusableBits.Interfaces;

namespace ReusableBits.Support {
	public class ResourceProvider : IResourceProvider {
		private readonly string		mResourceNamespace;
		private readonly string		mResourceDirectory;

		public ResourceProvider( string resourceNamespace, string resourceDirectory ) {
			mResourceNamespace = resourceNamespace;
			mResourceDirectory = resourceDirectory;
		}

		public BitmapImage RetrieveImage( string resourceName ) {
			var path = string.Format( "pack://application:,,,/{0};component/{1}/{2}", mResourceNamespace, mResourceDirectory, resourceName );

			return( new BitmapImage( new Uri( path )));
		}

		public object RetrieveTemplate( string templateName ) {
			return( Application.Current.TryFindResource( templateName ));
		}
	}
}
