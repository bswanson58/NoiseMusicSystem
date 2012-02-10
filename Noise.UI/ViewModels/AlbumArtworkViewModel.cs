using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class AlbumArtworkViewModel : DialogModelBase {
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly IResourceProvider	mResourceProvider;
		private readonly long				mAlbumId;
		private AlbumSupportInfo			mAlbumInfo;
		private	readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<UiAlbumExtra>	mAlbumImages;


		public AlbumArtworkViewModel( IAlbumProvider albumProvider, IResourceProvider resourceProvider, long albumId ) {
			mAlbumProvider = albumProvider;
			mResourceProvider = resourceProvider;
			mAlbumId = albumId;

			mAlbumImages = new ObservableCollectionEx<UiAlbumExtra>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveAlbumInfo((long)args.Argument );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetAlbumInfo( result.Result as AlbumSupportInfo );

			mBackgroundWorker.RunWorkerAsync( mAlbumId );
		}

		private AlbumSupportInfo RetrieveAlbumInfo( long albumId ) {
			return( mAlbumProvider.GetAlbumSupportInfo( albumId ));
		}

		private void SetAlbumInfo( AlbumSupportInfo albumInfo ) {
			mAlbumInfo = albumInfo;
			mAlbumImages.SuspendNotification();
			mAlbumImages.Clear();

			if( mAlbumInfo != null ) {
				if( mAlbumInfo.AlbumCovers != null ) {
					mAlbumImages.AddRange( from Artwork artwork in mAlbumInfo.AlbumCovers where !artwork.IsUserSelection select new UiAlbumExtra( artwork ));
				}

				if( mAlbumInfo.Artwork != null ) {
					mAlbumImages.AddRange( from Artwork artwork in mAlbumInfo.Artwork select new UiAlbumExtra( artwork ));
				}

				if( mAlbumInfo.Info != null ) {
					mAlbumImages.AddRange( from TextInfo info in mAlbumInfo.Info select new UiAlbumExtra( info, mResourceProvider.RetrieveImage(  "Text Document.png" )));
				}

				if( mAlbumImages.Count > 0 ) {
					CurrentImage = mAlbumImages[0];
				}
			}

			mAlbumImages.ResumeNotification();
		}

		[DependsUpon("CurrentImage")]
		public bool PreferredCover {
			get{ return( CurrentImage != null && CurrentImage.Artwork != null ? CurrentImage.Artwork.IsUserSelection : false ); }
			set {
				if(( CurrentImage != null ) &&
				   ( CurrentImage.Artwork != null ) &&
				   ( value ) &&
				   (!CurrentImage.Artwork.IsUserSelection )) {
					AlbumImages.Where( image => image.Artwork.IsUserSelection = false );
					CurrentImage.SetPreferredImage();
				}
			}
		}

		public UiAlbumExtra CurrentImage {
			get{ return( Get( () => CurrentImage )); }
			set{ Set( () => CurrentImage, value ); }
		}

		public IEnumerable<UiAlbumExtra> AlbumImages {
			get{ return( mAlbumImages ); }
		}
	}
}
