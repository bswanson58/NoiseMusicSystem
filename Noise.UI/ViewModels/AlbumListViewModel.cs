using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class AlbumListViewModel : AutomaticCommandBase,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string							cDisplaySortDescriptionss = "_displaySortDescriptions";
		private const string							cHideSortDescriptions = "_normal";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ISelectionState				mSelectionState;
		private readonly IObservableCollection<UiAlbum>	mAlbumList;
		private ICollectionView							mAlbumView;
		private readonly List<ViewSortStrategy>			mAlbumSorts;
		private DbArtist								mCurrentArtist;
		private TaskHandler								mAlbumRetrievalTaskHandler;
		private bool									mRetrievingAlbums;
		private long									mAlbumToSelect;
 
		public AlbumListViewModel( IEventAggregator eventAggregator, IAlbumProvider albumProvider, ISelectionState selectionState ) {
			mEventAggregator = eventAggregator;
			mAlbumProvider = albumProvider;
			mSelectionState = selectionState;

			mAlbumList = new BindableCollection<UiAlbum>();
			VisualStateName = cHideSortDescriptions;

			mAlbumSorts = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Name", ListSortDirection.Ascending ) } ),
													   new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "PublishedYear", ListSortDirection.Ascending ),
																														   new SortDescription( "Name", ListSortDirection.Ascending ) })};

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				var sortConfiguration = ( from config in mAlbumSorts where config.DisplayName == configuration.AlbumListSortOrder select config ).FirstOrDefault();

				SelectedSortDescription = sortConfiguration ?? mAlbumSorts[0];
			}
			else {
				SelectedSortDescription = mAlbumSorts[0];
			}

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseOpened args ) {
			FilterText = string.Empty;
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearAlbumList();

			VisualStateName = cHideSortDescriptions;
			FilterText = string.Empty;
		}

		private void OnArtistChanged( DbArtist artist ) {
			if( artist != null ) {
				RetrieveAlbums( artist );
			}
			else {
				ClearAlbumList();
			} 
		}

		private void OnAlbumChanged( DbAlbum album ) {
			mAlbumToSelect = Constants.cDatabaseNullOid;

			if( album != null ) {
				if( mRetrievingAlbums ) {
					mAlbumToSelect = album.DbId;
				}
				else {
					Set( () => SelectedAlbum, ( from a in mAlbumList where a.DbId == album.DbId select a ).FirstOrDefault());
				}
			}
		}

		public ICollectionView AlbumList {
			get{ 
				if( mAlbumView == null ) {
					mAlbumView = CollectionViewSource.GetDefaultView( mAlbumList );

					UpdateSorts();
					mAlbumView.Filter += OnAlbumFilter;
				}

				return( mAlbumView );
			}
		}

		public IEnumerable<ViewSortStrategy> SortDescriptions {
			get{ return( mAlbumSorts ); }
		} 

		public ViewSortStrategy SelectedSortDescription {
			get{ return( Get( () => SelectedSortDescription )); }
			set {
				Set( () => SelectedSortDescription, value );

				VisualStateName = cHideSortDescriptions;

				if( SelectedSortDescription != null ) {
					UpdateSorts();

					var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
					if( configuration != null ) {
						configuration.AlbumListSortOrder = SelectedSortDescription.DisplayName;

						NoiseSystemConfiguration.Current.Save( configuration );
					}
				}
			}
		}

		public string VisualStateName {
			get { return ( Get( () => VisualStateName )); }
			set { Set( () => VisualStateName, value ); }
		}

		private bool OnAlbumFilter( object node ) {
			var retValue = true;

			if(( node is UiAlbum ) &&
			   ( !string.IsNullOrWhiteSpace( FilterText ))) {
				var albumNode = node as UiAlbum;

				if(( albumNode.Name.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) &&
				   ( albumNode.Genre.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 )) {
					retValue = false;
				}
			}

			return ( retValue );
		}

		private void UpdateSorts() {
			if( mAlbumView != null ) {
				Execute.OnUIThread( () => {
					mAlbumView.SortDescriptions.Clear();

					foreach( var sortDescription in SelectedSortDescription.SortDescriptions ) {
						mAlbumView.SortDescriptions.Add( sortDescription );
					}
				} );
			}
		}

		public UiAlbum SelectedAlbum {
			get { return( Get( () => SelectedAlbum )); }
			set {
				Set( () => SelectedAlbum, value );

				if( value != null ) {
					mEventAggregator.Publish( new Events.AlbumFocusRequested( value.Artist, value.DbId ));
				}
			}
		}

		public string FilterText {
			get { return ( Get( () => FilterText )); }
			set {
				Execute.OnUIThread( () => {
					Set( () => FilterText, value );

					if( mAlbumView != null ) {
						mAlbumView.Refresh();
					}

					RaisePropertyChanged( () => FilterText );
				} );
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

		public void Execute_ToggleSortDisplay() {
			VisualStateName = VisualStateName == cHideSortDescriptions ? cDisplaySortDescriptionss : cHideSortDescriptions;
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
			mRetrievingAlbums = true;

			ClearAlbumList();

			mCurrentArtist = artist;
			RaisePropertyChanged( () => ArtistName );

			AlbumsRetrievalTaskHandler.StartTask( () => {
				using( var albums = mAlbumProvider.GetAlbumList( mCurrentArtist )) {
					SetAlbumList( albums.List );
				}

				mRetrievingAlbums = false;
			},
			() => {
				if( mAlbumToSelect != Constants.cDatabaseNullOid ) {
					Set( () => SelectedAlbum, ( from a in mAlbumList where a.DbId == mAlbumToSelect select a ).FirstOrDefault());

					mAlbumToSelect = Constants.cDatabaseNullOid;
				}
			},
			ex => {
				NoiseLogger.Current.LogException( "AlbumListViewModel:RetrieveAlbums", ex );

				mRetrievingAlbums = false;
			} );
		}

		private void ClearAlbumList() {
			mCurrentArtist = null;
			mAlbumList.Clear();
			FilterText = string.Empty;

			RaisePropertyChanged( () => ArtistName );
		}

		private void SetAlbumList( IEnumerable<DbAlbum> albumList ) {
			mAlbumList.IsNotifying = false;

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
