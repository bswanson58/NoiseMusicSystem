using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Noise.UI.Logging;
using Noise.UI.Resources;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class ArtistListViewModel : AutomaticCommandBase,
										 IHandle<Events.ArtistContentUpdated>, IHandle<Events.ArtistListFocusRequested>, IHandle<Events.GenreFocusRequested>,
                                         IHandle<Events.ArtistUserUpdate>, IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,
										 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string							cDisplaySortDescriptions = "_displaySortDescriptions";
		private const string							cHideSortDescriptions = "_normal";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly IPreferences					mPreferences;
		private readonly ISelectionState				mSelectionState;
		private readonly IArtistProvider				mArtistProvider;
		private readonly ITagManager					mTagManager;
        private readonly IPlayingItemHandler            mPlayingItemHandler;
		private readonly IRatings						mRatings;
        private readonly IPrefixedNameHandler           mPrefixedNameHandler;
		private	readonly Observal.Observer				mChangeObserver;
		private readonly BindableCollection<UiArtist>	mArtistList;
		private readonly ICollectionView				mArtistView;
		private readonly List<ViewSortStrategy>			mArtistSorts;
		private TaskHandler								mArtistRetrievalTaskHandler;

        public	ICollectionView							ArtistList => mArtistView;
        public	IEnumerable<ViewSortStrategy>			SortDescriptions => mArtistSorts;

		public ArtistListViewModel( IEventAggregator eventAggregator, IPreferences preferences, ISelectionState selectionState, IRatings ratings, IPrefixedNameHandler nameHandler,
									IArtistProvider artistProvider, ITagManager tagManager, IDatabaseInfo databaseInfo, IPlayingItemHandler playingItemHandler, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mPreferences = preferences;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;
            mPlayingItemHandler = playingItemHandler;
			mRatings = ratings;
            mPrefixedNameHandler = nameHandler;

			mArtistList = new BindableCollection<UiArtist>();
            mArtistView = CollectionViewSource.GetDefaultView( mArtistList );
            mArtistView.Filter += OnArtistFilter;
			mArtistView.CollectionChanged += OnArtistListChange;

			VisualStateName = cHideSortDescriptions;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mArtistSorts = new List<ViewSortStrategy> { new ViewSortStrategy( "Artist Name", new List<SortDescription> { new SortDescription( "Name", ListSortDirection.Ascending ) }),
														new ViewSortStrategy( "Unprefixed Artist Name", new List<SortDescription> { new SortDescription( "SortName", ListSortDirection.Ascending ) }),
                                                        new ViewSortStrategy( "Album Count", new List<SortDescription>{ new SortDescription( "AlbumCount", ListSortDirection.Descending ),
                                                        			                                                    new SortDescription( "SortName", ListSortDirection.Ascending ) }),
														new ViewSortStrategy( "Genre", new List<SortDescription> { new SortDescription( "Genre", ListSortDirection.Ascending ),
																												   new SortDescription( "SortName", ListSortDirection.Ascending )}),
		                                                new ViewSortStrategy( "Rating", new List<SortDescription> { new SortDescription("SortRating", ListSortDirection.Descending ),
		                                                                                                            new SortDescription( "SortName", ListSortDirection.Ascending ) }) };
            var configuration = mPreferences.Load<UserInterfacePreferences>();

			if( configuration != null ) {
				var sortConfiguration = ( from config in mArtistSorts where config.DisplayName == configuration.ArtistListSortOrder select config ).FirstOrDefault();

				if( sortConfiguration != null ) {
					SelectedSortDescription = sortConfiguration;
				}
				else {
					SelectedSortDescription = mPrefixedNameHandler.ArePrefixesEnabled ? mArtistSorts[1] : mArtistSorts[0];
				}
			}
			else {
				SelectedSortDescription = mPrefixedNameHandler.ArePrefixesEnabled ? mArtistSorts[1] : mArtistSorts[0];
			}

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
            mPlayingItemHandler.StartHandler( mArtistList );
			mEventAggregator.Subscribe( this );

            UpdateSorts();
            SetArtistFilter( ArtistFilterType.FilterText );

            if( databaseInfo.IsOpen ) {
				BuildArtistList();
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			ArtistFilter.ClearFilter();
			BuildArtistList();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearArtistList();

			VisualStateName = cHideSortDescriptions;
			ArtistFilter.ClearFilter();
		}

		public void Handle( Events.ArtistUserUpdate eventArgs ) {
			UpdateArtistNode( eventArgs.ArtistId );
		}

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			UpdateArtistNode( eventArgs.ArtistId );
		}

		public void Handle( Events.ArtistAdded eventArgs ) {
			var artist = mArtistProvider.GetArtist( eventArgs.ArtistId );

			if( artist != null ) {
				AddArtist( artist );

				RaisePropertyChanged( () => ArtistCount );
			}
		}

		public void Handle( Events.ArtistRemoved eventArgs ) {
			var artistNode = ( from UiArtist node in mArtistList where eventArgs.ArtistId == node.DbId select node ).FirstOrDefault();

			if( artistNode != null ) {
				mArtistList.Remove( artistNode );
				mChangeObserver.Release( artistNode );
	
				RaisePropertyChanged( () => ArtistCount );
			}
		}

		public void Handle( Events.ArtistListFocusRequested request ) {
			SetArtistFilter( ArtistFilterType.FilterArtistList );

			ArtistFilter.SetFilterList( request.ArtistList );
		}

		public void Handle( Events.GenreFocusRequested args ) {
			SetArtistFilter( ArtistFilterType.FilterGenre );

            ArtistFilter.FilterText = args.Genre;
        }

		private void OnArtistChanged( DbArtist artist ) {
			Set( () => SelectedArtist, artist != null ? ( from uiArtist in mArtistList where artist.DbId == uiArtist.DbId select uiArtist ).FirstOrDefault() : null );
		}

		private void BuildArtistList() {
			RetrieveArtists();
		}

		private void ClearArtistList() {
			foreach( var artist in mArtistList ) {
				mChangeObserver.Release( artist );
			}

			mArtistList.Clear();
			RaisePropertyChanged( () => ArtistCount );
		}

		[DependsUpon( "ArtistCount" )]
		public string ArtistListTitle => string.Format( ArtistCount == 0 ? StringResources.ArtistTitle : StringResources.ArtistTitlePlural, ArtistCount );

        public int ArtistCount {
			get {
				var retValue = 0;

				if( mArtistView is CollectionView view ) {
					retValue = view.Count;
				}

				return( retValue );
			}
		}

        public ViewSortStrategy SelectedSortDescription {
			get { return ( Get( () => SelectedSortDescription ) ); }
			set {
				Set( () => SelectedSortDescription, value );

				VisualStateName = cHideSortDescriptions;

				if( SelectedSortDescription != null ) {
					UpdateSorts();

					var configuration = mPreferences.Load<UserInterfacePreferences>();
					if( configuration != null ) {
						configuration.ArtistListSortOrder = SelectedSortDescription.DisplayName;

						mPreferences.Save( configuration );
					}
				}
			}
		}

		public string VisualStateName {
			get { return ( Get( () => VisualStateName ) ); }
			set { Set( () => VisualStateName, value ); }
		}

		public IArtistFilter ArtistFilter {
			get => Get(() => ArtistFilter );
			set {
				if( ArtistFilter != null ) {
                    ArtistFilter.FilterCleared -= OnFilterCleared;
                }

			    Set(() => ArtistFilter, value );

				if( ArtistFilter != null ) {
					ArtistFilter.FilterCleared += OnFilterCleared;
                }
			}
		}

		private void OnFilterCleared( object sender, EventArgs args ) {
			if( ArtistFilter != null ) {
				if(( ArtistFilter.FilterType == ArtistFilterType.FilterGenre ) ||
				   ( ArtistFilter.FilterType == ArtistFilterType.FilterArtistList )) {
					SetArtistFilter( ArtistFilterType.FilterText );
                }
			}
        }

		private void SetArtistFilter( ArtistFilterType ofType ) {
			ArtistFilter = ArtistFilterFactory.CreateArtistFilter( ofType, mArtistView );
        }

		private bool OnArtistFilter( object node ) {
			var retValue = true;

			if( node is UiArtist artistNode ) {
				retValue = ArtistFilter.DoesArtistMatch( artistNode );
			}

			return ( retValue );
		}

		private void UpdateSorts() {
			if( mArtistView != null ) {
				Execute.OnUIThread( () => {
					mArtistView.SortDescriptions.Clear();

					foreach( var sortDescription in SelectedSortDescription.SortDescriptions ) {
						mArtistView.SortDescriptions.Add( sortDescription );
					}
				} );
			}
		}

		public UiArtist SelectedArtist {
			get{ return( Get( () => SelectedArtist )); }
			set {
				Set( () => SelectedArtist, value );

				if( value != null ) {
					mEventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( value.DbId ));
				}
			}
		}

		private void OnArtistListChange( object sender, NotifyCollectionChangedEventArgs args ) {
            if(( mArtistView is CollectionView view ) &&
               ( view.Count == 1 )) {
                SelectedArtist = mArtistView.OfType<UiArtist>().FirstOrDefault();
            }

            RaisePropertyChanged( () => ArtistCount );
        }

        public void Execute_ToggleSortDisplay() {
			VisualStateName = VisualStateName == cHideSortDescriptions ? cDisplaySortDescriptions : cHideSortDescriptions;
		}

		internal TaskHandler ArtistsRetrievalTaskHandler {
			get {
				if( mArtistRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mArtistRetrievalTaskHandler = new TaskHandler());
				}

				return ( mArtistRetrievalTaskHandler );
			}
			set => mArtistRetrievalTaskHandler = value;
        }

		private void RetrieveArtists() {
			ArtistsRetrievalTaskHandler.StartTask( () => {
				using( var artists = mArtistProvider.GetArtistList()) {
					SetArtistList( artists.List );
				}
            },
            () => { },
            ex => mLog.LogException( "Retrieving Artists", ex ));
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist( OnGenreClicked );

			if( dbArtist != null ) {
				Mapper.Map( dbArtist, retValue );
				retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

                FormatSortPrefix( retValue );
			}

			return ( retValue );
		}

        private void OnGenreClicked( UiArtist artist ) {
			SetArtistFilter( ArtistFilterType.FilterGenre );

			ArtistFilter.FilterText = artist.Genre;
        }

		private void FormatSortPrefix( UiArtist artist ) {
            artist.DisplayName = mPrefixedNameHandler.FormatPrefixedName( artist.Name );
            artist.SortName = mPrefixedNameHandler.FormatSortingName( artist.Name );
		}

		private void SetArtistList( IEnumerable<DbArtist> artistList ) {
			mArtistList.IsNotifying = false;
			mArtistList.Clear();

			foreach( var artist in artistList ) {
				AddArtist( artist );
			}

			mArtistList.IsNotifying = true;
			mArtistList.Refresh();
			RaisePropertyChanged( () => ArtistCount );

            mPlayingItemHandler.UpdateList();
		}

		private void AddArtist( DbArtist artist ) {
			var uiArtist = TransformArtist( artist );

			mArtistList.Add( uiArtist );
			mChangeObserver.Add( uiArtist );
		}

		private void UpdateArtistNode( long artistId ) {
			var uiArtist = ( from UiArtist node in mArtistList where artistId == node.DbId select node ).FirstOrDefault();

			if( uiArtist != null ) {
				var artist = mArtistProvider.GetArtist( artistId );

				if( artist != null ) {
					mChangeObserver.Release( uiArtist );
					Mapper.Map( artist, uiArtist );
					uiArtist.DisplayGenre = mTagManager.GetGenre( artist.Genre );
					mChangeObserver.Add( uiArtist );

                    FormatSortPrefix( uiArtist );
				}
			}
		}

		private void OnArtistChanged( PropertyChangeNotification propertyNotification ) {
            if( propertyNotification.Source is UiBase notifier ) {
				var artist = mArtistProvider.GetArtist( notifier.DbId );

				if( artist != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mRatings.SetRating( artist, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mRatings.SetFavorite( artist, notifier.UiIsFavorite );
					}
				}
			}
		}
	}
}
