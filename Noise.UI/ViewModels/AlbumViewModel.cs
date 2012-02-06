using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : ViewModelBase, IActiveAware,
									IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.DatabaseItemChanged> {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IAlbumProvider				mAlbumProvider;
		private readonly ITrackProvider				mTrackProvider;
		private readonly IArtworkProvider			mArtworkProvider;
		private readonly ITagProvider				mTagProvider;
		private readonly ITagManager				mTagManager;
		private readonly IStorageFileProvider		mStorageFileProvider;
		private readonly IDialogService				mDialogService;
		private UiAlbum								mCurrentAlbum;
		private long								mRequestedAlbum;
		private readonly BitmapImage				mUnknownImage;
		private readonly BitmapImage				mSelectImage;
		private ImageScrubberItem					mCurrentAlbumCover;
		private bool								mIsActive;
		private string								mCategoryDisplay;
		private IEnumerable<DbTag>					mCategoryList;
		private readonly Observal.Observer			mChangeObserver;
		private readonly List<long>					mAlbumCategories;
		private TaskHandler<DbAlbum>				mAlbumTaskHandler;
 		private TaskHandler<AlbumSupportInfo>		mAlbumSupportTaskHandler;
		private TaskHandler<IEnumerable<DbTrack>>	mAlbumTracksTaskHandler;
		private TaskHandler<IEnumerable<long>>		mAlbumCategoriesTaskHandler; 

		public	TimeSpan						AlbumPlayTime { get; private set; }
		public	event EventHandler				IsActiveChanged;

		public AlbumViewModel( IEventAggregator eventAggregator, 
							   IAlbumProvider albumProvider, ITrackProvider trackProvider, IArtworkProvider artworkProvider,
							   ITagProvider tagProvider, IStorageFileProvider storageFileProvider,
							   ITagManager tagManager, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mArtworkProvider = artworkProvider;
			mStorageFileProvider = storageFileProvider;
			mTagProvider = tagProvider;
			mTagManager = tagManager;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

			mAlbumCategories = new List<long>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mUnknownImage = new BitmapImage( new Uri( "pack://application:,,,/Noise.UI;component/Resources/Unknown Album Image.png" ));
			mSelectImage = new BitmapImage( new Uri( "pack://application:,,,/Noise.UI;component/Resources/Select Album Image.png" ));

			using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
				mCategoryList = new List<DbTag>( tagList.List );
			}

			mIsActive = true;
		}

		public bool IsActive {
			get { return( mIsActive ); }
			set {
				mIsActive = value;

				if( mIsActive ) {
					UpdateAlbum( mRequestedAlbum );
				}
				else {
					ClearCurrentAlbum();
				}
			}
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
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

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value ); }
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( mCurrentAlbum != null ) {
				if( mCurrentAlbum.Artist != request.ArtistId ) {
					ClearCurrentAlbum();
				}
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			mRequestedAlbum = request.AlbumId;

			UpdateAlbum( request.AlbumId );

//			if(!mIsActive ) {
				mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.AlbumInfoView ));
//			}
		}

		private void UpdateAlbum( long albumId ) {
			ClearCurrentAlbum();

			if( albumId != Constants.cDatabaseNullOid ) {
				if(( mCurrentAlbum == null ) ||
				   ( mCurrentAlbum.DbId != albumId )) {
					RetrieveAlbumInfo( albumId );
					RetrieveAlbumSupportInfo( albumId );
					RetrieveTrackList( albumId );
					RetrieveAlbumCategories( albumId );
				}
			}
		}

		internal TaskHandler<DbAlbum> AlbumTaskHandler {
			get {
				if( mAlbumTaskHandler == null ) {
					mAlbumTaskHandler = new TaskHandler<DbAlbum>();
				}

				return( mAlbumTaskHandler );
			}

			set { mAlbumTaskHandler = value; }
		}
 
		private void RetrieveAlbumInfo( long albumId ) {
			AlbumTaskHandler.StartTask( () => mAlbumProvider.GetAlbum( albumId ),
										SetAlbum,
										ex => NoiseLogger.Current.LogException( "AlbumViewModel:RetrieveAlbumInfo", ex ));
		}

		private void SetAlbum( DbAlbum album ) {
		    mCurrentAlbum = TransformAlbum( album );

		    if( mCurrentAlbum != null ) {
				mChangeObserver.Add( mCurrentAlbum );
			}

			RaisePropertyChanged( () => Album );
		}

		internal TaskHandler<AlbumSupportInfo> AlbumSupportTaskHandler {
			get {
				if( mAlbumSupportTaskHandler == null ) {
					mAlbumSupportTaskHandler = new TaskHandler<AlbumSupportInfo>();
				}

				return( mAlbumSupportTaskHandler );
			}

			set { mAlbumSupportTaskHandler = value; }
		}

		private void RetrieveAlbumSupportInfo( long albumId ) {
			AlbumSupportTaskHandler.StartTask( () => mAlbumProvider.GetAlbumSupportInfo( albumId ),
												SetAlbumSupportInfo,
												ex => NoiseLogger.Current.LogException( "AlbumViewModel:RetrieveAlbumSupportInfo", ex ));
		}

		private void SetAlbumSupportInfo( AlbumSupportInfo supportInfo ) {
			mCurrentAlbumCover = SelectAlbumCover( supportInfo );
			SupportInfo = supportInfo;
		}

		internal TaskHandler<IEnumerable<DbTrack>> AlbumTracksTaskHandler {
			get {
				if( mAlbumTracksTaskHandler == null ) {
					mAlbumTracksTaskHandler = new TaskHandler<IEnumerable<DbTrack>>();
				}

				return( mAlbumTracksTaskHandler );
			}

			set { mAlbumTracksTaskHandler = value; }
		}

		private void RetrieveTrackList( long albumId ) {
			AlbumTracksTaskHandler.StartTask( () => { var retValue = new List<DbTrack>();
														using( var trackList = mTrackProvider.GetTrackList( albumId )) {
															retValue.AddRange( trackList.List );
														}

														return( retValue );
													},
											  SetTrackList,
											  ex => NoiseLogger.Current.LogException( "AlbumViewModel:RetrieveTrackList", ex ));
		}

		private void SetTrackList( IEnumerable<DbTrack> trackList ) {
			AlbumPlayTime = new TimeSpan();

			foreach( var dbTrack in trackList ) {
				AlbumPlayTime += dbTrack.Duration;
			}

			RaisePropertyChanged( () => AlbumPlayTime );
		}

		internal TaskHandler<IEnumerable<long>> AlbumCategoriesTaskHandler {
			get {
				if( mAlbumCategoriesTaskHandler == null ) {
					mAlbumCategoriesTaskHandler = new TaskHandler<IEnumerable<long>>();
				}

				return( mAlbumCategoriesTaskHandler );
			}

			set { mAlbumCategoriesTaskHandler = value; }
		}

		private void RetrieveAlbumCategories( long albumId ) {
			AlbumCategoriesTaskHandler.StartTask( () => { var retValue = new List<long>();
															using( var categoryList = mAlbumProvider.GetAlbumCategories( albumId )) {
																retValue.AddRange( categoryList.List );
															}

															return( retValue );
					                                    },
												  SetAlbumCategories,
												  ex => NoiseLogger.Current.LogException( "AlbumViewModel:RetrieveAlbumCategories", ex ));
		}

		private void SetAlbumCategories( IEnumerable<long> categories ) {
			mAlbumCategories.Clear();
			mAlbumCategories.AddRange( categories );
			mCategoryDisplay = "";
			foreach( var category in mAlbumCategories ) {
				foreach( var tag in mCategoryList ) {
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

		private ImageScrubberItem SelectAlbumCover( AlbumSupportInfo info ) {
			var	retValue = new ImageScrubberItem( 0, mUnknownImage, 0 );

			if( info != null ) {
				Artwork	cover = null;

				if(( info.AlbumCovers != null ) &&
				   ( info.AlbumCovers.GetLength( 0 ) > 0 )) {
					cover = (( from Artwork artwork in info.AlbumCovers where artwork.IsUserSelection select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
							 ( from Artwork artwork in info.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
								SupportInfo.AlbumCovers[0];
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
					retValue = new ImageScrubberItem( cover.DbId, CreateBitmap( cover.Image ), cover.Rotation );
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

		public void Handle( Events.DatabaseItemChanged eventArgs ) {
			var item = eventArgs.ItemChangedArgs.Item;

			if(( item is DbAlbum ) &&
			   ( mCurrentAlbum != null ) &&
			   ( eventArgs.ItemChangedArgs.Change == DbItemChanged.Update ) &&
			   ((item as DbAlbum).DbId == mCurrentAlbum.DbId )) {
				Execute.OnUIThread( () => Mapper.DynamicMap( item as DbAlbum, mCurrentAlbum ));
			}
		}

		public UiAlbum Album {
			get{ return( mCurrentAlbum ); }
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
					retValue.AddRange( SupportInfo.Artwork.Select( artwork => new ImageScrubberItem( artwork.DbId, CreateBitmap( artwork.Image ), artwork.Rotation )));
					retValue.AddRange( SupportInfo.AlbumCovers.Select( cover => new ImageScrubberItem( cover.DbId, CreateBitmap( cover.Image ), cover.Rotation )));
				}

				return( retValue );
			}
		}

		private static BitmapImage CreateBitmap( byte[] bytes ) {
			var stream = new MemoryStream( bytes );
			var bitmap = new BitmapImage();

			bitmap.BeginInit();
			bitmap.StreamSource = stream;
			bitmap.EndInit();

			return( bitmap );
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

		[DependsUpon( "Album" )]
		public bool AlbumValid {
			get{ return( mCurrentAlbum != null ); } 
		}

		public void Execute_EditAlbum() {
			if( mCurrentAlbum != null ) {
				using( var albumUpdate = mAlbumProvider.GetAlbumForUpdate( mCurrentAlbum.DbId )) {
					if( albumUpdate != null ) {
						var dialogModel = new AlbumEditDialogModel();

						if( mDialogService.ShowDialog( DialogNames.AlbumEdit, albumUpdate.Item, dialogModel ) == true ) {
							albumUpdate.Update();

							if( dialogModel.UpdateFileTags ) {
								GlobalCommands.SetMp3Tags.Execute( new SetMp3TagCommandArgs( albumUpdate.Item )
																						   { PublishedYear = albumUpdate.Item.PublishedYear });
							}
						}
					}
				}
			}
		}

		public void Execute_EditCategories() {
			if( mCurrentAlbum != null ) {
				var dialogModel = new CategorySelectionDialogModel( mCategoryList, mAlbumCategories, OnNewCategoryRequest );
				if( mDialogService.ShowDialog( DialogNames.CategorySelection, dialogModel ) == true ) {
					SetAlbumCategories( dialogModel.SelectedCategories );
					mAlbumProvider.SetAlbumCategories( mCurrentAlbum.Artist, mCurrentAlbum.DbId, dialogModel.SelectedCategories );
				}
			}
		}

		private IEnumerable<DbTag> OnNewCategoryRequest( object sender ) {
			IEnumerable<DbTag>	retValue = null;
			var					newCategory = new UiCategory();

			if( mDialogService.ShowDialog( DialogNames.CategoryEdit, newCategory ) == true ) {
				var tag = new DbTag( eTagGroup.User, newCategory.Name ) { Description = newCategory.Description };

				mTagProvider.AddTag( tag );

				using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
					mCategoryList = new List<DbTag>( tagList.List );

					retValue = mCategoryList;
				}
			}

			return( retValue );
		}

		[DependsUpon( "Album" )]
		public bool CanExecute_EditCategories() {
			return( mCurrentAlbum != null ); 
		}

		[DependsUpon( "SupportInfo" )]
		public string AlbumCategories {
			get{ return( mCategoryDisplay ); }
		}

		public void Execute_DisplayPictures() {
			if( CanExecute_DisplayPictures()) {
				var vm = new AlbumArtworkViewModel( mAlbumProvider, mCurrentAlbum.DbId );

				if( mDialogService.ShowDialog( DialogNames.AlbumArtworkDisplay, vm ) == true ) {
					foreach( var artwork in vm.AlbumImages ) {
						if( artwork.IsDirty ) {
							using( var update = mArtworkProvider.GetArtworkForUpdate( artwork.Artwork.DbId )) {
								if( artwork.Artwork.IsUserSelection ) {
									AlbumCover = new ImageScrubberItem( artwork.Artwork.DbId, CreateBitmap( artwork.Artwork.Image ), artwork.Artwork.Rotation );

									RaisePropertyChanged( () => AlbumCover );
								}

								update.Item.Rotation = artwork.Artwork.Rotation;
								update.Update();
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
				var path = mStorageFileProvider.GetAlbumPath( mCurrentAlbum.DbId );

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
