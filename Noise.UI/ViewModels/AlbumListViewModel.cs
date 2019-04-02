using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Resources;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class AlbumListViewModel : AutomaticCommandBase, IActiveAware,
										IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
										IHandle<Events.AlbumUserUpdate> {
		private const string							cDisplaySortDescriptionss = "_displaySortDescriptions";
		private const string							cHideSortDescriptions = "_normal";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly IPreferences					mPreferences;
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly IRatings						mRatings;
		private readonly IPlayCommand					mPlayCommand;
		private readonly ISelectionState				mSelectionState;
		private	readonly Observal.Observer				mChangeObserver;
		private readonly IObservableCollection<UiAlbum>	mAlbumList;
	    private readonly List<string>                   mSortPrefixes;
	    private readonly bool                           mEnableSortPrefixes;
		private ICollectionView							mAlbumView;
		private readonly List<ViewSortStrategy>			mAlbumSorts;
		private DbArtist								mCurrentArtist;
		private TaskHandler<IEnumerable<UiAlbum>>		mAlbumRetrievalTaskHandler;
		private CancellationTokenSource					mCancellationTokenSource;
		private bool									mIsActive;
		private bool									mRetrievingAlbums;
		private long									mAlbumToSelect;
 
		public	event	EventHandler					IsActiveChanged = delegate { };
	    public  IEnumerable<ViewSortStrategy>           SortDescriptions => mAlbumSorts;

        public AlbumListViewModel( IEventAggregator eventAggregator, IPreferences preferences, IAlbumProvider albumProvider, IPlayCommand playCommand, IRatings ratings,
								   ISelectionState selectionState, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mPreferences = preferences;
			mAlbumProvider = albumProvider;
			mRatings = ratings;
			mSelectionState = selectionState;
			mPlayCommand = playCommand;

			mAlbumList = new BindableCollection<UiAlbum>();
		    mSortPrefixes = new List<string>();
			VisualStateName = cHideSortDescriptions;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnAlbumChanged );

			mAlbumSorts = new List<ViewSortStrategy> { new ViewSortStrategy( "Album Name", new List<SortDescription> { new SortDescription( "Name", ListSortDirection.Ascending ) }),
			                                           new ViewSortStrategy( "Unprefixed Album Name", new List<SortDescription> { new SortDescription( "SortName", ListSortDirection.Ascending ) }),
                                                       new ViewSortStrategy( "Published Year", new List<SortDescription> { new SortDescription( "PublishedYear", ListSortDirection.Ascending ),
																														   new SortDescription( "SortName", ListSortDirection.Ascending ) }),
			                                           new ViewSortStrategy( "Rating", new List<SortDescription> { new SortDescription("SortRating", ListSortDirection.Descending ),
			                                                                                                       new SortDescription( "SortName", ListSortDirection.Ascending ) }) };

			var configuration = mPreferences.Load<UserInterfacePreferences>();
			if( configuration != null ) {
				var sortConfiguration = ( from config in mAlbumSorts where config.DisplayName == configuration.AlbumListSortOrder select config ).FirstOrDefault();

			    mEnableSortPrefixes = configuration.EnableSortPrefixes;
			    if (mEnableSortPrefixes) {
			        mSortPrefixes.AddRange(configuration.SortPrefixes.Split('|'));
			    }

                if ( sortConfiguration != null ) {
                    SelectedSortDescription = sortConfiguration;
                }
                else {
                    SelectedSortDescription = mEnableSortPrefixes ? mAlbumSorts[1] : mAlbumSorts[0];
                }
			}
            else {
				SelectedSortDescription = mAlbumSorts[0];
			}

		    mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
			mEventAggregator.Subscribe( this );
		}

		public bool IsActive {
			get => ( mIsActive );
		    set {
				mIsActive = value;

				IsActiveChanged( this, new EventArgs());

				if( mIsActive ) {
					if( mSelectionState.CurrentArtist != null ) {
						RetrieveAlbums( mSelectionState.CurrentArtist );
					}
				}
				else {
					CancelRetrievalTask();
					ClearAlbumList();
				}
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			FilterText = string.Empty;
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearAlbumList();

			VisualStateName = cHideSortDescriptions;
			FilterText = string.Empty;
		}

		public void Handle( Events.AlbumUserUpdate eventArgs ) {
			UpdateAlbumNode( eventArgs.AlbumId );
		}

		private void OnArtistChanged( DbArtist artist ) {
			if( artist != null ) {
				if( IsActive ) {
					RetrieveAlbums( artist );
				}
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

		public int AlbumCount {
			get {
				var retValue = 0;

				if( mAlbumView is CollectionView view ) {
					retValue = view.Count;
				}

				return( retValue );
			}
		}

		[DependsUpon( "AlbumCount" )]
		public string AlbumListTitle => ( string.Format( AlbumCount == 0 ? StringResources.AlbumTitle : StringResources.AlbumTitlePlural, AlbumCount ));

	    public ViewSortStrategy SelectedSortDescription {
			get{ return( Get( () => SelectedSortDescription )); }
			set {
				Set( () => SelectedSortDescription, value );

				VisualStateName = cHideSortDescriptions;

				if( SelectedSortDescription != null ) {
					UpdateSorts();

					var configuration = mPreferences.Load<UserInterfacePreferences>();
					if( configuration != null ) {
						configuration.AlbumListSortOrder = SelectedSortDescription.DisplayName;

						mPreferences.Save( configuration );
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

			if(( node is UiAlbum albumNode ) &&
			   ( !string.IsNullOrWhiteSpace( FilterText ))) {
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
					mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( value.Artist, value.DbId ));
				}
			}
		}

		public string FilterText {
			get { return ( Get( () => FilterText )); }
			set {
				Execute.OnUIThread( () => {
					Set( () => FilterText, value );

				    mAlbumView?.Refresh();

				    RaisePropertyChanged( () => FilterText );
					RaisePropertyChanged( () => AlbumCount );
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

		internal TaskHandler<IEnumerable<UiAlbum>> AlbumsRetrievalTaskHandler {
			get {
				if( mAlbumRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mAlbumRetrievalTaskHandler = new TaskHandler<IEnumerable<UiAlbum>>());
				}

				return ( mAlbumRetrievalTaskHandler );
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

		private void ClearCurrentTask() {
			mCancellationTokenSource = null;
		}

		private void RetrieveAlbums( DbArtist artist ) {
			CancelRetrievalTask();
			mRetrievingAlbums = true;

			var cancelToken = GenerateCanellationToken();

			AlbumsRetrievalTaskHandler.StartTask( 
					() =>  LoadAlbums( artist, cancelToken ),
					albumList => UpdateUi( albumList, artist ),
					ex => {
						mLog.LogException( $"Retrieving Albums for {artist}", ex );

						mRetrievingAlbums = false;

						ClearCurrentTask();
					}, cancelToken );
		}

		private void ClearAlbumList() {
			foreach( var album in mAlbumList ) {
				mChangeObserver.Release( album );
			}

			mCurrentArtist = null;
			mAlbumList.Clear();
			FilterText = string.Empty;

			RaisePropertyChanged( () => ArtistName );
			RaisePropertyChanged( () => AlbumCount );
		}

		private IEnumerable<UiAlbum> LoadAlbums( DbArtist forArtist, CancellationToken cancelToken ) {
			var retValue = new List<UiAlbum>();

			using( var albums = mAlbumProvider.GetAlbumList( forArtist )) {
				foreach( var album in albums.List ) {
					if(!cancelToken.IsCancellationRequested ) {
						var uiAlbum = TransformAlbum( album );

						retValue.Add( uiAlbum );
						mChangeObserver.Add( uiAlbum );
					}
					else {
						retValue.Clear();

						break;
					}
				}
			}

			return( retValue );
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum( OnPlay );

			if( dbAlbum != null ) {
				Mapper.Map( dbAlbum, retValue );

			    if( mEnableSortPrefixes ) {
			        FormatSortPrefix( retValue );
			    }
			}

            return ( retValue );
		}

	    private void FormatSortPrefix( UiAlbum album ) {
	        if( mSortPrefixes != null ) {
	            foreach( string prefix in mSortPrefixes ) {
	                if( album.Name.StartsWith( prefix + " ", StringComparison.CurrentCultureIgnoreCase )) {
	                    album.SortName = album.Name.Remove( 0, prefix.Length ).Trim();
	                    album.DisplayName = "(" + album.Name.Insert( prefix.Length, ")" );

	                    break;
	                }
	            }
	        }
	    }

	    private void UpdateUi( IEnumerable<UiAlbum> albumList, DbArtist artist ) {
			ClearAlbumList();
			mAlbumList.AddRange( albumList );
			mCurrentArtist = artist;

			RaisePropertyChanged( () => ArtistName );
			RaisePropertyChanged( () => AlbumCount );

			if( mAlbumToSelect != Constants.cDatabaseNullOid ) {
				Set( () => SelectedAlbum, ( from a in mAlbumList where a.DbId == mAlbumToSelect select a ).FirstOrDefault());

				mAlbumToSelect = Constants.cDatabaseNullOid;
			}

			mRetrievingAlbums = false;
			ClearCurrentTask();
		}

		private void OnPlay( long albumId ) {
			var album = mAlbumProvider.GetAlbum( albumId );

			if( album != null ) {
				mPlayCommand.Play( album );
			}
		}

		private void UpdateAlbumNode( long albumId ) {
			var uiAlbum = ( from UiAlbum node in mAlbumList where albumId == node.DbId select node ).FirstOrDefault();

			if( uiAlbum != null ) {
				var album = mAlbumProvider.GetAlbum( albumId );

				if( album != null ) {
					mChangeObserver.Release( uiAlbum );
					Mapper.Map( album, uiAlbum );
					mChangeObserver.Add( uiAlbum );
				}
			}
		}

		private void OnAlbumChanged( PropertyChangeNotification propertyNotification ) {
		    if( propertyNotification.Source is UiBase notifier ) {
				var album = mAlbumProvider.GetAlbum( notifier.DbId );

				if( album != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mRatings.SetRating( album, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mRatings.SetFavorite( album, notifier.UiIsFavorite );
					}
				}
			}
		}
	}
}
