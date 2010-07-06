using System;
using System.Net;

namespace Noise.Core.DataProviders {
	internal class ImageDownloader {
		private readonly WebClient	mWebClient;
		private readonly object		mParentObject;
		private readonly Action<object, byte[]>	mOnDownloadComplete;

		public ImageDownloader( string uriString, object parentObject, Action<object, byte[]> onCompleted ) {
			mWebClient = new WebClient();
			mParentObject = parentObject;
			mOnDownloadComplete = onCompleted;

			mWebClient.DownloadDataCompleted += OnDownloadCompleted;
			mWebClient.DownloadDataAsync( new Uri( uriString ));
		}

		private void OnDownloadCompleted( object sender, DownloadDataCompletedEventArgs e ) {
			if(( e.Error == null ) &&
			   ( e.Cancelled == false )) {
				mOnDownloadComplete( mParentObject, e.Result );

				mWebClient.DownloadDataCompleted -= OnDownloadCompleted;
			}			
		}
	}
}
