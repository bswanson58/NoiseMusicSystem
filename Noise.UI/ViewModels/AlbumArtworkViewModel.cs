using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class AlbumArtworkViewModel : PropertyChangeBase, IDialogAware {
		public	const string						cAlbumIdParameter = "albumId";

		private readonly IAlbumProvider				mAlbumProvider;
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly IResourceProvider			mResourceProvider;
		private readonly IPlatformLog				mLog;
		private readonly ObservableCollectionEx<UiAlbumExtra>	mAlbumImages;
        private AlbumSupportInfo					mAlbumInfo;
		private TaskHandler<AlbumSupportInfo>		mAlbumInfoHandler;
		private long								mAlbumId;
		private UiAlbumExtra						mCurrentImage;

        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose;
        public	IEnumerable<UiAlbumExtra>			AlbumImages => mAlbumImages;

		public AlbumArtworkViewModel( IAlbumProvider albumProvider, IArtworkProvider artworkProvider, IResourceProvider resourceProvider, IPlatformLog log ) {
			mAlbumProvider = albumProvider;
			mArtworkProvider = artworkProvider;
			mResourceProvider = resourceProvider;
			mLog = log;

			mAlbumImages = new ObservableCollectionEx<UiAlbumExtra>();

            Ok = new DelegateCommand( OnOk );
			Cancel = new DelegateCommand( OnCancel );

			Title = "Album Artwork";
		}

        public void OnDialogOpened( IDialogParameters parameters ) {
			mAlbumId = parameters.GetValue<long>( cAlbumIdParameter );

			if( mAlbumId != Constants.cDatabaseNullOid ) {
				RetrieveAlbumInfo( mAlbumId );
            }
        }

        internal TaskHandler<AlbumSupportInfo> AlbumRetrievalTaskHandler {
            get {
                if( mAlbumInfoHandler == null ) {
                    Execute.OnUIThread( () => mAlbumInfoHandler = new TaskHandler<AlbumSupportInfo>());
                }

                return( mAlbumInfoHandler );
            }

            set => mAlbumInfoHandler = value;
        }

        private void RetrieveAlbumInfo( long albumId ) {
			AlbumRetrievalTaskHandler.StartTask( () => mAlbumProvider.GetAlbumSupportInfo( albumId ), SetAlbumInfo, ex => { mLog.LogException( "AlbumArtworkViewModel", ex ); });
		}

		private void SetAlbumInfo( AlbumSupportInfo albumInfo ) {
			mAlbumInfo = albumInfo;
			mAlbumImages.Clear();

			if( mAlbumInfo != null ) {
                var images = new List<Artwork>();

				if( mAlbumInfo.AlbumCovers != null ) {
					images.AddRange( mAlbumInfo.AlbumCovers );
				}

                if( mAlbumInfo.Artwork != null ) {
                    images.AddRange( mAlbumInfo.Artwork );
                }

                mAlbumImages.AddRange( images.DistinctBy( i => i.Name ).OrderBy( i => i.Name ).Select( artwork => new UiAlbumExtra( artwork )));

                if( mAlbumInfo.Info != null ) {
					mAlbumImages.AddRange( from TextInfo info in mAlbumInfo.Info orderby info.Name select new UiAlbumExtra( info, mResourceProvider.RetrieveImage( "Text Document.png" )));
				}

                CurrentImage = mAlbumImages.FirstOrDefault( a => a.Artwork?.IsUserSelection == true ) ?? mAlbumImages.FirstOrDefault();
            }
		}

		public bool PreferredCover {
			get => CurrentImage?.Artwork != null && CurrentImage.Artwork.IsUserSelection;
            set {
				if( CurrentImage?.Artwork != null ) {
					AlbumImages.ForEach( image => image.UserSelection( false ));

                    CurrentImage.UserSelection( value );
				}
			}
		}

		private void UpdateArtwork() {
            mAlbumImages.Where( a => a.IsDirty ).Select( a => a.Artwork ).ForEach( artwork => {
                using( var update = mArtworkProvider.GetArtworkForUpdate( artwork.DbId )) {
                    if( update?.Item != null ) {
                        update.Item.Rotation = artwork.Rotation;
                        update.Item.IsUserSelection = artwork.IsUserSelection;

                        update.Update();
                    }
                }
            });
        }

		public UiAlbumExtra CurrentImage {
			get => mCurrentImage;
            set {
                mCurrentImage = value;

				RaisePropertyChanged( () => CurrentImage );
				RaisePropertyChanged( () => PreferredCover );
            }
		}

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
			UpdateArtwork();

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
	}
}
