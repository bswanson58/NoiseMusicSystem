using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class AlbumArtworkViewModel : DialogModelBase {
		private readonly IUnityContainer	mContainer;
		private readonly long				mAlbumId;
		private AlbumSupportInfo			mAlbumInfo;
		private	readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<BitmapImage>	mAlbumImages;


		public AlbumArtworkViewModel( IUnityContainer container, long albumId ) {
			mContainer = container;
			mAlbumId = albumId;

			mAlbumImages = new ObservableCollectionEx<BitmapImage>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveAlbumInfo((long)args.Argument );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetAlbumInfo( result.Result as AlbumSupportInfo );

			mBackgroundWorker.RunWorkerAsync( mAlbumId );
		}

		private AlbumSupportInfo RetrieveAlbumInfo( long albumId ) {
			var noiseManager = mContainer.Resolve<INoiseManager>();

			return( noiseManager.DataProvider.GetAlbumSupportInfo( albumId ));
		}

		private void SetAlbumInfo( AlbumSupportInfo albumInfo ) {
			mAlbumInfo = albumInfo;
			mAlbumImages.SuspendNotification();
			mAlbumImages.Clear();

			if( mAlbumInfo != null ) {
				if( mAlbumInfo.AlbumCovers != null ) {
					mAlbumImages.AddRange( from DbArtwork artwork in mAlbumInfo.AlbumCovers select CreateBitmap( artwork.Image ));
				}

				if( mAlbumInfo.Artwork != null ) {
					mAlbumImages.AddRange( from DbArtwork artwork in mAlbumInfo.Artwork select CreateBitmap( artwork.Image ));
				}

				if( mAlbumImages.Count > 0 ) {
					CurrentImage = mAlbumImages[0];
				}
			}

			mAlbumImages.ResumeNotification();
		}

		public BitmapImage CurrentImage {
			get{ return( Get( () => CurrentImage )); }
			set{ Set( () => CurrentImage, value ); }
		}

		public IEnumerable<BitmapImage> AlbumImages {
			get{ return( mAlbumImages ); }
		}

		private static BitmapImage CreateBitmap( byte[] bytes ) {
			var stream = new MemoryStream( bytes );
			var bitmap = new BitmapImage();

			bitmap.BeginInit();
			bitmap.StreamSource = stream;
			bitmap.EndInit();

			return( bitmap );
		}
	}
}
