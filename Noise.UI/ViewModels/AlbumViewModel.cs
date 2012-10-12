using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Interfaces;
using ReusableBits.Ui.ValueConverters;

namespace Noise.UI.ViewModels {
	internal class AlbumEditRequest : InteractionRequestData<AlbumEditDialogModel> {
		public AlbumEditRequest( AlbumEditDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumArtworkDisplayInfo : InteractionRequestData<AlbumArtworkViewModel> {
		public AlbumArtworkDisplayInfo( AlbumArtworkViewModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumCategoryEditInfo : InteractionRequestData<CategorySelectionDialogModel> {
		public AlbumCategoryEditInfo( CategorySelectionDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumViewModel : ViewModelBase,
									IHandle<Events.DatabaseClosing>,
									IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.AlbumUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IArtworkProvider		mArtworkProvider;
		private readonly IResourceProvider		mResourceProvider;
		private readonly ITagProvider			mTagProvider;
		private readonly ITagManager			mTagManager;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private UiAlbum							mCurrentAlbum;
		private readonly BitmapImage			mUnknownImage;
		private readonly BitmapImage			mSelectImage;
		private ImageScrubberItem				mCurrentAlbumCover;
		private string							mCategoryDisplay;
		private readonly Observal.Observer		mChangeObserver;
		private readonly List<long>				mAlbumCategories;
		private TaskHandler						mAlbumRetrievalTaskHandler;

		private readonly InteractionRequest<AlbumEditRequest>			mAlbumEditRequest; 
		private readonly InteractionRequest<AlbumArtworkDisplayInfo>	mAlbumArtworkDisplayRequest;
		private readonly InteractionRequest<AlbumCategoryEditInfo>		mAlbumCategoryEditRequest; 

		public	TimeSpan						AlbumPlayTime { get; private set; }

		public AlbumViewModel( IEventAggregator eventAggregator, IResourceProvider resourceProvider,
							   IAlbumProvider albumProvider, ITrackProvider trackProvider, IArtworkProvider artworkProvider,
							   ITagProvider tagProvider, IStorageFolderSupport storageFolderSupport, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mArtworkProvider = artworkProvider;
			mStorageFolderSupport = storageFolderSupport;
			mResourceProvider = resourceProvider;
			mTagProvider = tagProvider;
			mTagManager = tagManager;

			mEventAggregator.Subscribe( this );

			mAlbumCategories = new List<long>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mUnknownImage = mResourceProvider.RetrieveImage( "Unknown Album Image.png" );
			mSelectImage = mResourceProvider.RetrieveImage( "Select Album Image.png" );

			mAlbumEditRequest = new InteractionRequest<AlbumEditRequest>();
			mAlbumArtworkDisplayRequest = new InteractionRequest<AlbumArtworkDisplayInfo>();
			mAlbumCategoryEditRequest = new InteractionRequest<AlbumCategoryEditInfo>();
		}

		private void ClearCurrentAlbum() {
			if( mCurrentAlbum != null ) {
				mChangeObserver.Release( mCurrentAlbum );
			}
			mCurrentAlbum = null;
			SupportInfo = null;
			mCurrentAlbumCover = new ImageScrubberItem( 0, mUnknownImage, 0 );

			RaisePropertyChanged( () => Album );
			RaisePropertyChanged( () => SupportInfo );
			RaisePropertyChanged( () => AlbumPlayTime );
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentAlbum();
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( mCurrentAlbum != null ) {
				if( mCurrentAlbum.Artist != request.ArtistId ) {
					ClearCurrentAlbum();
				}
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			UpdateAlbum( request.AlbumId );

			mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.AlbumInfoView ));
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

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private void SetAlbum( DbAlbum album ) {
			if( album != null ) {
			    mCurrentAlbum = TransformAlbum( album );

				mChangeObserver.Add( mCurrentAlbum );
			}
			else {
				ClearCurrentAlbum();
			}

			RaisePropertyChanged( () => Album );
		}

		private void SetAlbumSupportInfo( AlbumSupportInfo supportInfo ) {
			Execute.OnUIThread( () => {
				mCurrentAlbumCover = SelectAlbumCover( supportInfo );
				SupportInfo = supportInfo;
			});
		}

		private void SetTrackList( IEnumerable<DbTrack> trackList ) {
			AlbumPlayTime = new TimeSpan();

			foreach( var dbTrack in trackList ) {
				AlbumPlayTime += dbTrack.Duration;
			}

			RaisePropertyChanged( () => AlbumPlayTime );
		}

		private void SetAlbumCategories( IEnumerable<long> categories ) {
			mAlbumCategories.Clear();
			mAlbumCategories.AddRange( categories );
			mCategoryDisplay = "";

			var categoryList = new List<DbTag>();
			using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
				if(( tagList != null ) &&
				   ( tagList.List != null )) {
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

			set{ mAlbumRetrievalTaskHandler = value; }
		}

		private void RetrieveAlbum( long albumId ) {
			AlbumRetrievalTaskHandler.StartTask( () => {
					SetAlbum( mAlbumProvider.GetAlbum( albumId ));
					SetAlbumSupportInfo( mAlbumProvider.GetAlbumSupportInfo( albumId ));

					using( var trackList = mTrackProvider.GetTrackList( albumId )) {
						if(( trackList != null ) &&
						   ( trackList.List != null )) {
							SetTrackList( trackList.List );
						}
					}

					using( var categoryList = mAlbumProvider.GetAlbumCategories( albumId )) {
						if(( categoryList != null ) &&
						   ( categoryList.List != null )) {
							SetAlbumCategories( categoryList.List );
						}
					}
				},
				() => { },
				ex => NoiseLogger.Current.LogException( "AlbumViewModel:RetrieveAlbum", ex )
			); 
		}

		private ImageScrubberItem SelectAlbumCover( AlbumSupportInfo info ) {
			var	retValue = new ImageScrubberItem( 0, mUnknownImage, 0 );

			if( info != null ) {
				Artwork	cover = null;

				if(( info.AlbumCovers != null ) &&
				   ( info.AlbumCovers.GetLength( 0 ) > 0 )) {
					cover = (( from Artwork artwork in info.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								info.AlbumCovers[0];
				}

				if(( cover == null ) &&
				   ( info.Artwork != null ) &&
				   ( info.Artwork.GetLength( 0 ) > 0 )) {
					cover = ( from Artwork artwork in info.Artwork
							  where artwork.Name.IndexOf( "front", StringComparison.InvariantCultureIgnoreCase ) >= 0 select artwork ).FirstOrDefault();

					if(( cover == null ) &&
					   ( info.Artwork.GetLength( 0 ) == 1 )) {
						cover = info.Artwork[0];
					}
				}

				if( cover != null ) {
					retValue = new ImageScrubberItem( cover.DbId, ByteImageConverter.CreateBitmap( cover.Image ), cover.Rotation );
				}
				else {
					if(( info.Artwork != null ) &&
					   ( info.Artwork.GetLength( 0 ) > 0 )) {
						retValue = new ImageScrubberItem( 1, mSelectImage, 0 );
					}
				}
			}

			return( retValue );
		}

		private static void OnNodeChanged( PropertyChangeNotification propertyNotification ) {

			if( propertyNotification.Source is UiBase ) {
				var item = propertyNotification.Source as UiBase;

				if( propertyNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( item.DbId, item.UiRating ));
				}
				if( propertyNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( item.DbId, item.UiIsFavorite ));
				}
			}
		}

		public void Handle( Events.AlbumUserUpdate eventArgs ) {
			if(( mCurrentAlbum != null ) &&
			   ( eventArgs.AlbumId == mCurrentAlbum.DbId )) {
				Mapper.DynamicMap( mAlbumProvider.GetAlbum( eventArgs.AlbumId ), mCurrentAlbum );
			}
		}

		public UiAlbum Album {
			get{ return( mCurrentAlbum ); }
		}

		[DependsUpon( "Album" )]
		public bool AlbumValid {
			get{ return( mCurrentAlbum != null ); } 
		}

		public string AlbumCategories {
			get{ return( mCategoryDisplay ); }
		}

		[DependsUpon( "AlbumCategories" )]
		public bool HaveAlbumCategories {
			get{ return( mAlbumCategories.Any()); }
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ImageScrubberItem AlbumCover {
			get { return( mCurrentAlbumCover ); }
			set {
				if( value.Id != mCurrentAlbumCover.Id ) {
					mCurrentAlbumCover = value;

					GlobalCommands.SetAlbumCover.Execute( new SetAlbumCoverCommandArgs( mCurrentAlbum.DbId, value.Id ));
				}
			}
		}

		[DependsUpon( "SupportInfo" )]
		public IEnumerable<ImageScrubberItem> AlbumArtwork {
			get {
				var	retValue = new List<ImageScrubberItem>();

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Artwork != null )) {
					retValue.AddRange( SupportInfo.Artwork.Select( artwork => new ImageScrubberItem( artwork.DbId, ByteImageConverter.CreateBitmap( artwork.Image ), artwork.Rotation )));
					retValue.AddRange( SupportInfo.AlbumCovers.Select( cover => new ImageScrubberItem( cover.DbId, ByteImageConverter.CreateBitmap( cover.Image ), cover.Rotation )));
				}

				return( retValue );
			}
		}

		public void Execute_PlayAlbum() {
			if( mCurrentAlbum != null ) {
				GlobalCommands.PlayAlbum.Execute( mAlbumProvider.GetAlbum( mCurrentAlbum.DbId ));
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_PlayAlbum() {
			return( mCurrentAlbum != null ); 
		}

		public IInteractionRequest AlbumEditRequest {
			get{ return( mAlbumEditRequest ); }
		}

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

		public IInteractionRequest AlbumCategoryEditRequest {
			get{ return( mAlbumCategoryEditRequest ); }
		}

		public void Execute_EditCategories() {
			if( mCurrentAlbum != null ) {
				var dialogModel = new CategorySelectionDialogModel( mTagProvider, mAlbumCategories );

				mAlbumCategoryEditRequest.Raise( new AlbumCategoryEditInfo( dialogModel ), OnCategoriesEdited );
			}
		}

		private void OnCategoriesEdited( AlbumCategoryEditInfo confirmation ) {
			if( confirmation.Confirmed ) {
				SetAlbumCategories( confirmation.ViewModel.SelectedCategories );
				mAlbumProvider.SetAlbumCategories( mCurrentAlbum.Artist, mCurrentAlbum.DbId, confirmation.ViewModel.SelectedCategories );
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_EditCategories() {
			return( mCurrentAlbum != null ); 
		}

		public IInteractionRequest AlbumArtworkDisplayRequest {
			get{ return( mAlbumArtworkDisplayRequest ); }
		}

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

			return( retValue );
		}

		public void Execute_OpenAlbumFolder() {
			if( mCurrentAlbum != null ) {
				var path = mStorageFolderSupport.GetAlbumPath( mCurrentAlbum.DbId );

				if(!string.IsNullOrWhiteSpace( path )) {
					mEventAggregator.Publish( new Events.LaunchRequest( path ));
				}
			}
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_OpenAlbumFolder() {
			return( mCurrentAlbum != null );
		}
	}
}
