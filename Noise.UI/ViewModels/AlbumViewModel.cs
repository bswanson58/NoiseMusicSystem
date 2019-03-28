using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.ValueConverters;

namespace Noise.UI.ViewModels {
	internal class AlbumEditRequest : InteractionRequestData<AlbumEditDialogModel> {
		public AlbumEditRequest( AlbumEditDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumArtworkDisplayInfo : InteractionRequestData<AlbumArtworkViewModel> {
		public AlbumArtworkDisplayInfo( AlbumArtworkViewModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumViewModel : AutomaticCommandBase,
									IHandle<Events.DatabaseClosing>, IHandle<Events.AlbumUserUpdate> {
		private const string						cAllTracks = "Entire Album";

		private readonly IEventAggregator			mEventAggregator;
		private readonly IUiLog						mLog;
		private readonly ISelectionState			mSelectionState;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly IAlbumArtworkProvider		mAlbumArtworkProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly IResourceProvider			mResourceProvider;
		private readonly ITagProvider				mTagProvider;
		private readonly ITagManager				mTagManager;
		private readonly IPlayCommand				mPlayCommand;
		private readonly IRatings					mRatings;
		private readonly IStorageFolderSupport		mStorageFolderSupport;
		private UiAlbum								mCurrentAlbum;
		private readonly BitmapImage				mUnknownImage;
		private readonly BitmapImage				mSelectImage;
		private ImageScrubberItem					mCurrentAlbumCover;
		private string								mCategoryDisplay;
		private readonly Observal.Observer			mChangeObserver;
		private readonly List<long>					mAlbumCategories;
		private readonly BindableCollection<string>	mVolumeNames;
		private string								mCurrentVolumeName;
		private TaskHandler							mAlbumRetrievalTaskHandler;
		private CancellationTokenSource				mCancellationTokenSource;

		private readonly InteractionRequest<AlbumEditRequest>			mAlbumEditRequest; 
		private readonly InteractionRequest<AlbumArtworkDisplayInfo>	mAlbumArtworkDisplayRequest;

		public	TimeSpan						AlbumPlayTime { get; private set; }

		public AlbumViewModel( IEventAggregator eventAggregator, IResourceProvider resourceProvider, ISelectionState selectionState, IRatings ratings,
							   IAlbumProvider albumProvider, ITrackProvider trackProvider, IAlbumArtworkProvider albumArtworkProvider, IArtworkProvider artworkProvider,
							   ITagProvider tagProvider, IStorageFolderSupport storageFolderSupport, ITagManager tagManager, IPlayCommand playCommand, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mAlbumProvider = albumProvider;
			mAlbumArtworkProvider = albumArtworkProvider;
			mTrackProvider = trackProvider;
			mArtworkProvider = artworkProvider;
			mStorageFolderSupport = storageFolderSupport;
			mResourceProvider = resourceProvider;
			mTagProvider = tagProvider;
			mTagManager = tagManager;
			mRatings = ratings;
			mPlayCommand = playCommand;

			mEventAggregator.Subscribe( this );

			mAlbumCategories = new List<long>();
			mVolumeNames = new BindableCollection<string>();
			mCurrentVolumeName = string.Empty;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mUnknownImage = mResourceProvider.RetrieveImage( "Unknown Album Image.png" );
			mSelectImage = mResourceProvider.RetrieveImage( "Select Album Image.png" );

			mAlbumEditRequest = new InteractionRequest<AlbumEditRequest>();
			mAlbumArtworkDisplayRequest = new InteractionRequest<AlbumArtworkDisplayInfo>();

			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
			OnAlbumChanged( mSelectionState.CurrentAlbum );
		}

		private void ClearCurrentAlbum() {
			if( mCurrentAlbum != null ) {
				mChangeObserver.Release( mCurrentAlbum );
			}
			mCurrentAlbum = null;
			SupportInfo = null;
			mVolumeNames.Clear();
			mCurrentVolumeName = string.Empty;
			mCurrentAlbumCover = new ImageScrubberItem( 0, mUnknownImage, 0 );

			RaisePropertyChanged( () => Album );
			RaisePropertyChanged( () => SupportInfo );
			RaisePropertyChanged( () => AlbumPlayTime );
			RaisePropertyChanged( () => HasMultipleVolumes );
			RaisePropertyChanged( () => VolumeNames );
			RaisePropertyChanged( () => CurrentVolumeName );
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
			var retValue = new UiAlbum();

			Mapper.Map( dbAlbum, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private void SetAlbum( DbAlbum album ) {
			if( album != null ) {
				if( mCurrentAlbum != null ) {
					mChangeObserver.Release( mCurrentAlbum );
				}

			    mCurrentAlbum = TransformAlbum( album );
				mChangeObserver.Add( mCurrentAlbum );
			}
			else {
				ClearCurrentAlbum();
			}

			RaisePropertyChanged( () => Album );
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
			var	retValue = new ImageScrubberItem( 0, mUnknownImage, 0 );
			var cover = mAlbumArtworkProvider.GetAlbumCover( forAlbum );

			if( cover != null ) {
				retValue = new ImageScrubberItem( cover.DbId, ByteImageConverter.CreateBitmap( cover.Image ), cover.Rotation );
			}

			return( retValue );
		}

		private void SetTrackList( IEnumerable<DbTrack> tracks ) {
			var trackList = tracks.ToList();

			AlbumPlayTime = TimeSpan.FromMilliseconds( trackList.Sum( track => track.DurationMilliseconds ));

			mVolumeNames.AddRange(( from track in trackList 
									where !string.IsNullOrWhiteSpace( track.VolumeName ) 
									orderby track.VolumeName 
									select track.VolumeName ).Distinct());

			if( mVolumeNames.Any()) {
				mVolumeNames.Insert( 0, cAllTracks );
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
			mCategoryDisplay = "";

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

		private CancellationToken GenerateCanellationToken() {
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

			var cancellationToken = GenerateCanellationToken();

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
				Mapper.Map( mAlbumProvider.GetAlbum( eventArgs.AlbumId ), mCurrentAlbum );
				mChangeObserver.Add( mCurrentAlbum );
			}
		}

		public UiAlbum Album => ( mCurrentAlbum );

        [DependsUpon( "Album" )]
		public bool AlbumValid => ( mCurrentAlbum != null );

        public string AlbumCategories => ( mCategoryDisplay );

        [DependsUpon( "AlbumCategories" )]
		public bool HaveAlbumCategories => ( mAlbumCategories.Any());

        public AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ImageScrubberItem AlbumCover {
			get => ( mCurrentAlbumCover );
            set {
				if( value.Id != mCurrentAlbumCover.Id ) {
					mCurrentAlbumCover = value;

					mAlbumArtworkProvider.SetAlbumCover( mCurrentAlbum.DbId, mCurrentAlbumCover.Id );
				}
			}
		}

		[DependsUpon( "SupportInfo" )]
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

		public void Execute_PlayAlbum() {
			if( mCurrentAlbum != null ) {
				mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ));
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_PlayAlbum() {
			return( mCurrentAlbum != null ); 
		}

		public void Execute_PlayVolume() {
			if( mCurrentAlbum != null ) {
				if( string.Equals( cAllTracks, mCurrentVolumeName )) {
					mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ));
				}
				else {
					mPlayCommand.Play( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ), mCurrentVolumeName );
				}
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_PlayVolume() {
			return( mCurrentAlbum != null );
		}

		public bool HasMultipleVolumes => ( mVolumeNames.Any());

        public BindableCollection<string> VolumeNames => ( mVolumeNames );

        public string CurrentVolumeName {
			get => ( mCurrentVolumeName );
            set {
                mCurrentVolumeName = value;
                mSelectionState.SetCurrentAlbumVolume( cAllTracks.Equals( mCurrentVolumeName ) ? string.Empty : mCurrentVolumeName );
            }
        }

		public IInteractionRequest AlbumEditRequest => ( mAlbumEditRequest );

        public void Execute_EditAlbum() {
			if( CanExecute_EditAlbum()) {
				var album = new UiAlbum { DbId = mCurrentAlbum.DbId,  Name = mCurrentAlbum.Name, PublishedYear = mCurrentAlbum.PublishedYear };
				var dialogModel = new AlbumEditDialogModel( album );

				mAlbumEditRequest.Raise( new AlbumEditRequest( dialogModel ), OnAlbumEdited );
			}
		}

		private void OnAlbumEdited( AlbumEditRequest confirmation ) {
			if( confirmation.Confirmed ) {
				using( var updater = mAlbumProvider.GetAlbumForUpdate( confirmation.ViewModel.Album.DbId )) {
					if(( updater != null ) &&
					   ( updater.Item != null )) {
						updater.Item.Name = confirmation.ViewModel.Album.Name;
						updater.Item.PublishedYear = confirmation.ViewModel.Album.PublishedYear;

						updater.Update();

						if( confirmation.ViewModel.UpdateFileTags ) {
							GlobalCommands.SetMp3Tags.Execute( new SetMp3TagCommandArgs( updater.Item ) { PublishedYear = updater.Item.PublishedYear });
						}
					}
				}

				if(( mCurrentAlbum != null ) &&
				   ( mCurrentAlbum.DbId == confirmation.ViewModel.Album.DbId )) {
					mCurrentAlbum.Name = confirmation.ViewModel.Album.Name;
					mCurrentAlbum.PublishedYear = confirmation.ViewModel.Album.PublishedYear;
				}
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_EditAlbum() {
			return( mCurrentAlbum != null );
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_EditCategories() {
			return( mCurrentAlbum != null ); 
		}

		public IInteractionRequest AlbumArtworkDisplayRequest => ( mAlbumArtworkDisplayRequest );

        public void Execute_DisplayPictures() {
			if( CanExecute_DisplayPictures()) {
				var vm = new AlbumArtworkViewModel( mAlbumProvider, mResourceProvider, mCurrentAlbum.DbId );

				mAlbumArtworkDisplayRequest.Raise( new AlbumArtworkDisplayInfo( vm ), AfterArtworkDisplayed );
			}
		}

		private void AfterArtworkDisplayed( AlbumArtworkDisplayInfo confirmation ) {
			if( confirmation.Confirmed ) {
				foreach( var artwork in confirmation.ViewModel.AlbumImages ) {
					if( artwork.IsDirty ) {
						using( var update = mArtworkProvider.GetArtworkForUpdate( artwork.Artwork.DbId )) {
							if(( update != null ) &&
							   ( update.Item != null )) {
								update.Item.Rotation = artwork.Artwork.Rotation;
								update.Update();
							}

							if( artwork.Artwork.IsUserSelection ) {
								AlbumCover = new ImageScrubberItem( artwork.Artwork.DbId, ByteImageConverter.CreateBitmap( artwork.Artwork.Image ), artwork.Artwork.Rotation );

								RaisePropertyChanged( () => AlbumCover );
                                RaisePropertyChanged( () => AlbumArtwork );
							}
						}
					}
				}
			}
		}

		[DependsUpon( "SupportInfo" )]
		public bool CanExecute_DisplayPictures() {
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

		public void Execute_OpenAlbumFolder() {
			if( mCurrentAlbum != null ) {
				var path = mStorageFolderSupport.GetAlbumPath( mCurrentAlbum.DbId );

				if(!string.IsNullOrWhiteSpace( path )) {
					mEventAggregator.PublishOnUIThread( new Events.LaunchRequest( path ));
				}
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_OpenAlbumFolder() {
			return( mCurrentAlbum != null );
		}
	}
}
