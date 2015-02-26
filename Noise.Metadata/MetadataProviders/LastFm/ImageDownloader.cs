using System;
using System.Net;

namespace Noise.Metadata.MetadataProviders.LastFm {
	internal class ImageDownloader {
		private readonly WebClient			mWebClient;
		private readonly string				mArtistName;
		private readonly Action< string, byte[]>	mOnDownloadComplete;

		public static ImageDownloader StartDownload( string uriString, string artistName, Action<string, byte[]> onCompleted ) {
			return new ImageDownloader( uriString, artistName, onCompleted );
		}

		public ImageDownloader( string uriString, string artistName, Action<string, byte[]> onCompleted ) {
			mArtistName = artistName;
			mWebClient = new WebClient();
			mOnDownloadComplete = onCompleted;

			mWebClient.DownloadDataCompleted += OnDownloadCompleted;
			mWebClient.DownloadDataAsync( new Uri( uriString ));
		}

		private void OnDownloadCompleted( object sender, DownloadDataCompletedEventArgs e ) {
			if(( e.Error == null ) &&
			   ( e.Cancelled == false )) {
				mOnDownloadComplete( mArtistName, e.Result );
			}			

			mWebClient.DownloadDataCompleted -= OnDownloadCompleted;
		}
	}
}
