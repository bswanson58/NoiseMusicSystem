using System;
using System.Net;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class ImageDownloader {
		private readonly WebClient			mWebClient;
		private readonly IArtworkProvider	mArtworkProvider;
		private readonly long				mArtworkId;
		private readonly Action<IArtworkProvider, long, byte[]>	mOnDownloadComplete;

		public ImageDownloader( string uriString, IArtworkProvider artworkProvider, long artworkId, Action<IArtworkProvider, long, byte[]> onCompleted ) {
			mWebClient = new WebClient();
			mArtworkProvider = artworkProvider;
			mArtworkId = artworkId;
			mOnDownloadComplete = onCompleted;

			mWebClient.DownloadDataCompleted += OnDownloadCompleted;
			mWebClient.DownloadDataAsync( new Uri( uriString ));
		}

		private void OnDownloadCompleted( object sender, DownloadDataCompletedEventArgs e ) {
			if(( e.Error == null ) &&
			   ( e.Cancelled == false )) {
				mOnDownloadComplete( mArtworkProvider, mArtworkId, e.Result );

				mWebClient.DownloadDataCompleted -= OnDownloadCompleted;
			}			
		}
	}
}
