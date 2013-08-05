using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class AlbumListViewModel : AutomaticPropertyBase,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISelectionState				mSelectionState;
		private readonly IObservableCollection<UiAlbum>	mAlbumList;
		private ICollectionView							mAlbumView;
		private readonly List<ViewSortStrategy>			mAlbumSorts;
		private ViewSortStrategy						mCurrentAlbumSort;
		private DbArtist								mCurrentArtist;
		private TaskHandler								mAlbumRetrievalTaskHandler;
 
		public AlbumListViewModel( IEventAggregator eventAggregator, IAlbumProvider albumProvider, ISelectionState selectionState ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mSelectionState = selectionState;

			mAlbumList = new BindableCollection<UiAlbum>();

			mAlbumSorts = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Name", ListSortDirection.Ascending ) } ),
													   new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "PublishedYear", ListSortDirection.Ascending ),
																														   new SortDescription( "Name", ListSortDirection.Ascending ) })};
			mCurrentAlbumSort = mAlbumSorts[0];

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearAlbumList();
		}

		private void OnArtistChanged( DbArtist artist ) {
			if( artist != null ) {
				RetrieveAlbums( artist );
			}
			else {
				ClearAlbumList();
			} 
		}

		public ICollectionView AlbumList {
			get{ 
				if( mAlbumView == null ) {
					mAlbumView = CollectionViewSource.GetDefaultView( mAlbumList );

					UpdateSorts();
				}

				return( mAlbumView );
			}
		}

		private void UpdateSorts() {
			if( mAlbumView != null ) {
				Execute.OnUIThread( () => {
					mAlbumView.SortDescriptions.Clear();

					foreach( var sortDescrition in mCurrentAlbumSort.SortDescriptions ) {
						mAlbumView.SortDescriptions.Add( sortDescrition );
					}
				} );
			}
		}

		public UiAlbum SelectedAlbum {
			get { return( Get( () => SelectedAlbum )); }
			set {
				Set( () => SelectedAlbum, value );

				if( value != null ) {
					mEventAggregator.Publish( new Events.AlbumFocusRequested( value.Artist, value.DbId ) );
				}
			}
		}

		public string ArtistName {
			get {
				var retValue = string.Empty;

				if( mCurrentArtist != null ) {
					retValue = mCurrentArtist.Name;
				}

				return( retValue );
			}
		}

		internal TaskHandler AlbumsRetrievalTaskHandler {
			get {
				if( mAlbumRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mAlbumRetrievalTaskHandler = new TaskHandler() );
				}

				return ( mAlbumRetrievalTaskHandler );
			}
			set { mAlbumRetrievalTaskHandler = value; }
		}

		private void RetrieveAlbums( DbArtist artist ) {
			mCurrentArtist = artist;
			RaisePropertyChanged( () => ArtistName );

			AlbumsRetrievalTaskHandler.StartTask( () => {
				using( var albums = mAlbumProvider.GetAlbumList( mCurrentArtist )) {
					SetAlbumList( albums.List );
				}
			},
			() => { },
			ex => NoiseLogger.Current.LogException( "AlbumListViewModel:RetrieveAlbums", ex ) );
		}

		private void ClearAlbumList() {
			mCurrentArtist = null;
			mAlbumList.Clear();

			RaisePropertyChanged( () => ArtistName );
		}

		private void SetAlbumList( IEnumerable<DbAlbum> albumList ) {
			mAlbumList.IsNotifying = false;
			mAlbumList.Clear();

			foreach( var album in albumList ) {
				mAlbumList.Add( TransformAlbum( album ));
			}

			mAlbumList.IsNotifying = true;
			mAlbumList.Refresh();
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum( OnPlay );

			if( dbAlbum != null ) {
				Mapper.DynamicMap( dbAlbum, retValue );
			}

			return ( retValue );
		}

		private void OnPlay( long albumId ) {
			var album = mAlbumProvider.GetAlbum( albumId );

			if( album != null ) {
				GlobalCommands.PlayAlbum.Execute( album );
			}
		}
	}
}
