using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Views;
using Observal.Extensions;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.ValueConverters;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : AutomaticPropertyBase,
									IHandle<Events.DatabaseClosing>, IHandle<Events.AlbumUserUpdate> {
		private const string						cAllTracks = "Entire Album";

		private readonly IEventAggregator			mEventAggregator;
		private readonly IUiLog						mLog;
		private readonly ISelectionState			mSelectionState;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly IAlbumArtworkProvider		mAlbumArtworkProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly ITagProvider				mTagProvider;
		private readonly ITagManager				mTagManager;
		private readonly IPlayCommand				mPlayCommand;
		private readonly IPlayingItemHandler		mPlayingItemHandler;
		private readonly IRatings					mRatings;
		private readonly IStorageFolderSupport		mStorageFolderSupport;
		private readonly IDialogService				mDialogService;
		private UiAlbum								mCurrentAlbum;
		private ImageScrubberItem					mCurrentAlbumCover;
		private string								mCategoryDisplay;
		private readonly Observal.Observer			mChangeObserver;
		private readonly List<long>					mAlbumCategories;
		private string								mCurrentVolumeName;
		private TaskHandler							mAlbumRetrievalTaskHandler;
		private CancellationTokenSource				mCancellationTokenSource;

        public	UiAlbum								Album => mCurrentAlbum;
        public	bool								AlbumValid => mCurrentAlbum != null;
        public	bool								ArtworkValid { get; private set; }
		public	TimeSpan							AlbumPlayTime { get; private set; }
        public	bool								HasMultipleVolumes => VolumeNames.Any();
        public	BindableCollection<string>			VolumeNames { get; }
        public	string								AlbumCategories => mCategoryDisplay;
        public	bool								HaveAlbumCategories => mAlbumCategories.Any();

		public	DelegateCommand						PlayAlbum { get; }
		public	DelegateCommand						PlayVolume { get; }
		public	DelegateCommand						DisplayPictures { get; }
		public	DelegateCommand						EditAlbum { get; }
		public	DelegateCommand						OpenAlbumFolder { get; }

		public AlbumViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, IRatings ratings,
							   IAlbumProvider albumProvider, ITrackProvider trackProvider, IAlbumArtworkProvider albumArtworkProvider,
							   ITagProvider tagProvider, IStorageFolderSupport storageFolderSupport, ITagManager tagManager, IPlayCommand playCommand, 
                               IPlayingItemHandler playingItemHandler, IDialogService dialogService, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mAlbumProvider = albumProvider;
			mAlbumArtworkProvider = albumArtworkProvider;
			mTrackProvider = trackProvider;
			mStorageFolderSupport = storageFolderSupport;
			mTagProvider = tagProvider;
			mTagManager = tagManager;
			mRatings = ratings;
			mPlayCommand = playCommand;
			mPlayingItemHandler = playingItemHandler;
			mDialogService = dialogService;

			PlayAlbum = new DelegateCommand( OnPlayAlbum, CanPlayAlbum );
			PlayVolume = new DelegateCommand( OnPlayVolume, CanPlayVolume );
			DisplayPictures = new DelegateCommand( OnDisplayPictures, CanDisplayPictures );
			EditAlbum = new DelegateCommand( OnEditAlbum, CanEditAlbum );
			OpenAlbumFolder = new DelegateCommand( OnOpenAlbumFolder, CanOpenAlbumFolder );

			mPlayingItemHandler.StartHandler( () => Album );

			mEventAggregator.Subscribe( this );

			mAlbumCategories = new List<long>();
			VolumeNames = new BindableCollection<string>();
			mCurrentVolumeName = string.Empty;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
			OnAlbumChanged( mSelectionState.CurrentAlbum );
		}

		private void ClearCurrentAlbum() {
			if( mCurrentAlbum != null ) {
				mChangeObserver.Release( mCurrentAlbum );
			}
			mCurrentAlbum = null;
			SupportInfo = null;
			VolumeNames.Clear();
			mCurrentVolumeName = string.Empty;
			mCurrentAlbumCover = null;

			RaisePropertyChanged( () => Album );
			RaisePropertyChanged( () => AlbumValid );
			RaisePropertyChanged( () => SupportInfo );
			RaisePropertyChanged( () => AlbumPlayTime );
			RaisePropertyChanged( () => HasMultipleVolumes );
			RaisePropertyChanged( () => VolumeNames );
			RaisePropertyChanged( () => CurrentVolumeName );

			PlayAlbum.RaiseCanExecuteChanged();
			PlayVolume.RaiseCanExecuteChanged();
			DisplayPictures.RaiseCanExecuteChanged();
			EditAlbum.RaiseCanExecuteChanged();
			OpenAlbumFolder.RaiseCanExecuteChanged();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentAlbum();
		}

		private void OnAlbumChanged( DbAlbum album ) {
			if( album != null ) {
				UpdateAlbum( album.DbId );
			}
			else {
				ClearCurrentAlbum();
			}
		}

		private void UpdateAlbum( long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearCurrentAlbum();
			}
			else {
				if(( mCurrentAlbum == null ) ||
				   ( mCurrentAlbum.DbId != albumId )) {
					ClearCurrentAlbum();

					RetrieveAlbum( albumId );
				}
			}
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum( dbAlbum );

			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private void SetAlbum( DbAlbum album ) {
			if( album != null ) {
				if( mCurrentAlbum != null ) {
					mChangeObserver.Release( mCurrentAlbum );
				}

			    mCurrentAlbum = TransformAlbum( album );
				mPlayingItemHandler.UpdateItem();
				mChangeObserver.Add( mCurrentAlbum );
			}
			else {
				ClearCurrentAlbum();
			}

			RaisePropertyChanged( () => Album );
            RaisePropertyChanged( () => AlbumValid );
            PlayAlbum.RaiseCanExecuteChanged();
            PlayVolume.RaiseCanExecuteChanged();
            DisplayPictures.RaiseCanExecuteChanged();
            EditAlbum.RaiseCanExecuteChanged();
            OpenAlbumFolder.RaiseCanExecuteChanged();
		}

		private void SetAlbumInfo( DbAlbum album ) {
			Execute.OnUIThread( () => {
				SetAlbum( album );
				mCurrentAlbumCover = SelectAlbumCover( album );

                RaisePropertyChanged( () => AlbumArtwork );
                RaisePropertyChanged( () => AlbumCover );
			});
		}

		private ImageScrubberItem SelectAlbumCover( DbAlbum forAlbum ) {
			var	retValue = default( ImageScrubberItem );
			var cover = mAlbumArtworkProvider.GetAlbumCover( forAlbum );

			if(( cover?.Image != null ) &&
			   ( cover.Image.Length > 0 )) {
				retValue = new ImageScrubberItem( cover.DbId, ByteImageConverter.CreateBitmap( cover.Image ), cover.Rotation );
			}

			ArtworkValid = retValue != null;
			RaisePropertyChanged( () => ArtworkValid );

			return( retValue );
		}

		private void SetTrackList( IEnumerable<DbTrack> tracks ) {
			var trackList = tracks.ToList();

			AlbumPlayTime = TimeSpan.FromMilliseconds( trackList.Sum( track => track.DurationMilliseconds ));

			VolumeNames.AddRange(( from track in trackList 
								   where !string.IsNullOrWhiteSpace( track.VolumeName ) 
                                   orderby track.VolumeName 
                                   select track.VolumeName ).Distinct());

			if( VolumeNames.Any()) {
				VolumeNames.Insert( 0, cAllTracks );
				mCurrentVolumeName = cAllTracks;
			}

			RaisePropertyChanged( () => AlbumPlayTime );
			RaisePropertyChanged( () => HasMultipleVolumes );
			RaisePropertyChanged( () => VolumeNames );
			RaisePropertyChanged( () => CurrentVolumeName );
		}

		private void SetAlbumCategories( IEnumerable<long> categories ) {
			mAlbumCategories.Clear();
			mAlbumCategories.AddRange( categories );
			mCategoryDisplay = String.Empty;

			var categoryList = new List<DbTag>();
			using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
				if(tagList?.List != null) {
					categoryList.AddRange( tagList.List );
				}
			}

			foreach( var category in mAlbumCategories ) {
				foreach( var tag in categoryList ) {
					if( tag.DbId == category ) {
						if(!string.IsNullOrWhiteSpace( mCategoryDisplay )) {
							mCategoryDisplay += ", " + tag.Name;
						}
						else {
							mCategoryDisplay = tag.Name;
						}
					}
				}
			}

			RaisePropertyChanged( () => AlbumCategories );
			RaisePropertyChanged( () => HaveAlbumCategories );
		}

		internal TaskHandler AlbumRetrievalTaskHandler {
			get {
				if( mAlbumRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mAlbumRetrievalTaskHandler = new TaskHandler());
				}

				return( mAlbumRetrievalTaskHandler );
			}

			set => mAlbumRetrievalTaskHandler = value;
        }

		private CancellationToken GenerateCancellationToken() {
			mCancellationTokenSource = new CancellationTokenSource();

			return( mCancellationTokenSource.Token );
		}

		private void CancelRetrievalTask() {
			if( mCancellationTokenSource != null ) {
				mCancellationTokenSource.Cancel();
				mCancellationTokenSource = null;
			}
		}

		private void RetrieveAlbum( long albumId ) {
			CancelRetrievalTask();

			var cancellationToken = GenerateCancellationToken();

			AlbumRetrievalTaskHandler.StartTask( () => {
					if(!cancellationToken.IsCancellationRequested ) {
						SetAlbumInfo(  mAlbumProvider.GetAlbum( albumId ));
					}

					if(!cancellationToken.IsCancellationRequested ) {
						using( var trackList = mTrackProvider.GetTrackList( albumId )) {
							if(trackList?.List != null) {
								SetTrackList( trackList.List );
							}
						}
					}

					if(!cancellationToken.IsCancellationRequested ) {
						using( var categoryList = mAlbumProvider.GetAlbumCategories( albumId )) {
							if(categoryList?.List != null) {
								SetAlbumCategories( categoryList.List );
							}
						}
					}

                    if(!cancellationToken.IsCancellationRequested ) {
                        SupportInfo = mAlbumProvider.GetAlbumSupportInfo( albumId );
                    }
				},
				() => { },
				ex => mLog.LogException( $"Retrieving Album:{albumId}", ex ),
				cancellationToken ); 
		}

		private void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			if( propertyNotification.Source is UiBase item ) {
                var album = mAlbumProvider.GetAlbum( item.DbId );

				if( album != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mRatings.SetRating( album, item.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mRatings.SetFavorite( album, item.UiIsFavorite );
					}
				}
			}
		}

		public void Handle( Events.AlbumUserUpdate eventArgs ) {
			if(( mCurrentAlbum != null ) &&
			   ( eventArgs.AlbumId == mCurrentAlbum.DbId )) {
				mChangeObserver.Release( mCurrentAlbum );
				mCurrentAlbum.UpdateFromSource( mAlbumProvider.GetAlbum( eventArgs.AlbumId ));
				mChangeObserver.Add( mCurrentAlbum );
			}
		}

        public AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
                Set( () => SupportInfo, value );

				RaisePropertyChanged( () => AlbumCover );
				RaisePropertyChanged( () => AlbumArtwork );
				DisplayPictures.RaiseCanExecuteChanged();
            }
		}

		public ImageScrubberItem AlbumCover {
			get => mCurrentAlbumCover;
            set {
				if( value.Id != mCurrentAlbumCover.Id ) {
					mCurrentAlbumCover = value;

					mAlbumArtworkProvider.SetAlbumCover( mCurrentAlbum.DbId, mCurrentAlbumCover.Id );
				}
			}
		}

		public IEnumerable<ImageScrubberItem> AlbumArtwork {
			get {
				var	retValue = new List<ImageScrubberItem>();

				if(SupportInfo?.Artwork != null) {
					retValue.AddRange( SupportInfo.Artwork.Select( artwork => new ImageScrubberItem( artwork.DbId, ByteImageConverter.CreateBitmap( artwork.Image ), artwork.Rotation )));
					retValue.AddRange( SupportInfo.AlbumCovers.Select( cover => new ImageScrubberItem( cover.DbId, ByteImageConverter.CreateBitmap( cover.Image ), cover.Rotation )));
				}
                else {
				    if( mCurrentAlbumCover != null ) {
				        retValue.Add( mCurrentAlbumCover );
				    }
				}

				return( retValue );
			}
		}

		private void OnPlayAlbum() {
			if( mCurrentAlbum != null ) {
				mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ));
			}
		}

		private bool CanPlayAlbum() {
			return( mCurrentAlbum != null ); 
		}

		private void OnPlayVolume() {
			if( mCurrentAlbum != null ) {
				if( string.Equals( cAllTracks, mCurrentVolumeName )) {
					mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ));
				}
				else {
					mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ), mCurrentVolumeName );
				}
			}
		}

		private bool CanPlayVolume() {
			return( mCurrentAlbum != null );
		}

        public string CurrentVolumeName {
			get => ( mCurrentVolumeName );
            set {
                mCurrentVolumeName = value;
                mSelectionState.SetCurrentAlbumVolume( cAllTracks.Equals( mCurrentVolumeName ) ? string.Empty : mCurrentVolumeName );
            }
        }

        private void OnEditAlbum() {
			if(( CanEditAlbum()) &&
			   ( mCurrentAlbum != null )) {
				var parameters = new DialogParameters{{ AlbumEditDialogModel.cAlbumIdParameter, mCurrentAlbum.DbId }};

				mDialogService.ShowDialog( nameof( AlbumEditDialog ), parameters, result => {
					if( result.Result == ButtonResult.OK ) {
						var albumId = result.Parameters.GetValue<long>( AlbumEditDialogModel.cAlbumIdParameter );

						if( albumId != Constants.cDatabaseNullOid ) {
                            ClearCurrentAlbum();

                            RetrieveAlbum( albumId );
                            mEventAggregator.PublishOnUIThread( new Events.AlbumStructureChanged( albumId ));
                        }
                    }
                });
			}
		}

		private bool CanEditAlbum() {
			return( mCurrentAlbum != null );
		}

        private void OnDisplayPictures() {
			if( CanDisplayPictures()) {
				var parameters = new DialogParameters{{ AlbumArtworkViewModel.cAlbumIdParameter, mCurrentAlbum.DbId }};

				mDialogService.ShowDialog( nameof( AlbumArtworkView ), parameters, result => {
					if( result.Result == ButtonResult.OK ) {
						RetrieveAlbum( mCurrentAlbum.DbId );
                    }
                });
			}
		}

        private bool CanDisplayPictures() {
			var retValue = false;

			if( SupportInfo != null ) {
				if(( SupportInfo.Artwork != null ) &&
				   ( SupportInfo.Artwork.GetLength( 0 ) > 0 )) {
					retValue = true;
				}

				if(( SupportInfo.Info != null ) &&
				   ( SupportInfo.Info.GetLength( 0 ) > 0 )) {
					retValue = true;
				}
			}
            else {
			    retValue = mCurrentAlbumCover != null;
			}

			return( retValue );
		}

		public void OnOpenAlbumFolder() {
			if( mCurrentAlbum != null ) {
				var path = mStorageFolderSupport.GetAlbumPath( mCurrentAlbum.DbId );

				if(!string.IsNullOrWhiteSpace( path )) {
					mEventAggregator.PublishOnUIThread( new Events.LaunchRequest( path ));
				}
			}
		}

		public bool CanOpenAlbumFolder() {
			return( mCurrentAlbum != null );
		}
	}
}
