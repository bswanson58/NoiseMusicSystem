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
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistListViewModel : AutomaticCommandBase,
									   IHandle<Events.ArtistUserUpdate>, IHandle<Events.ArtistContentUpdated>,
									   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,
									   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const string							cDisplaySortDescriptionss = "_displaySortDescriptions";
		private const string							cHideSortDescriptions = "_normal";

		private readonly IEventAggregator				mEventAggregator;
		private readonly IPreferences					mPreferences;
		private readonly ISelectionState				mSelectionState;
		private readonly IArtistProvider				mArtistProvider;
		private readonly ITagManager					mTagManager;
		private	readonly Observal.Observer				mChangeObserver;
		private readonly BindableCollection<UiArtist>	mArtistList;
		private ICollectionView							mArtistView;
		private readonly bool							mEnableSortPrefixes;
		private readonly List<string>					mSortPrefixes;
		private readonly List<ViewSortStrategy>			mArtistSorts;
		private TaskHandler								mArtistRetrievalTaskHandler;

		public ArtistListViewModel( IEventAggregator eventAggregator, IPreferences preferences, ISelectionState selectionState ,
									IArtistProvider artistProvider, ITagManager tagManager, IDatabaseInfo databaseInfo ) {
			mEventAggregator = eventAggregator;
			mPreferences = preferences;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;

			mArtistList = new BindableCollection<UiArtist>();
			mSortPrefixes = new List<string>();
			VisualStateName = cHideSortDescriptions;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			var configuration = mPreferences.Load<UserInterfacePreferences>();
			if( configuration != null ) {
				mEnableSortPrefixes = configuration.EnableSortPrefixes;

				if( mEnableSortPrefixes ) {
					mSortPrefixes.AddRange( configuration.SortPrefixes.Split( '|' ));
				}
			}

			mArtistSorts = new List<ViewSortStrategy> { new ViewSortStrategy( "Artist Name", new List<SortDescription> { new SortDescription( "Name", ListSortDirection.Ascending ) }),
														new ViewSortStrategy( "Unprefixed Artist Name", new List<SortDescription> { new SortDescription( "SortName", ListSortDirection.Ascending ) }),
														new ViewSortStrategy( "Genre", new List<SortDescription> { new SortDescription( "Genre", ListSortDirection.Ascending ),
																												   new SortDescription( "SortName", ListSortDirection.Ascending )}) };
			if( configuration != null ) {
				var sortConfiguration = ( from config in mArtistSorts where config.DisplayName == configuration.ArtistListSortOrder select config ).FirstOrDefault();

				if( sortConfiguration != null ) {
					SelectedSortDescription = sortConfiguration;
				}
				else {
					SelectedSortDescription = mEnableSortPrefixes ? mArtistSorts[1] : mArtistSorts[0];
				}
			}
			else {
				SelectedSortDescription = mEnableSortPrefixes ? mArtistSorts[1] : mArtistSorts[0];
			}

			mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
			mEventAggregator.Subscribe( this );

			if( databaseInfo.IsOpen ) {
				BuildArtistList();
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			FilterText = string.Empty;
			BuildArtistList();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearArtistList();

			VisualStateName = cHideSortDescriptions;
			FilterText = string.Empty;
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
			}
		}

		public void Handle( Events.ArtistRemoved eventArgs ) {
			var artistNode = ( from UiArtist node in mArtistList where eventArgs.ArtistId == node.DbId select node ).FirstOrDefault();

			if( artistNode != null ) {
				mArtistList.Remove( artistNode );
				mChangeObserver.Release( artistNode );
			}
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
		}

		public ICollectionView ArtistList {
			get{ 
				if( mArtistView == null ) {
					mArtistView = CollectionViewSource.GetDefaultView( mArtistList );

					UpdateSorts();
					mArtistView.Filter += OnArtistFilter;
				}

				return( mArtistView );
			}
		}

		public IEnumerable<ViewSortStrategy> SortDescriptions {
			get { return ( mArtistSorts ); }
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

		private bool OnArtistFilter( object node ) {
			var retValue = true;

			if(( node is UiArtist ) &&
			   (!string.IsNullOrWhiteSpace( FilterText ))) {
				var artistNode = node as UiArtist;

				if(( artistNode.Name.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) &&
				   ( artistNode.Genre.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 )) {
					retValue = false;
				}
			}

			return ( retValue );
		}

		private void UpdateSorts() {
			if( mArtistView != null ) {
				Execute.OnUIThread( () => {
					mArtistView.SortDescriptions.Clear();

					foreach( var sortDescrition in SelectedSortDescription.SortDescriptions ) {
						mArtistView.SortDescriptions.Add( sortDescrition );
					}
				} );
			}
		}

		public UiArtist SelectedArtist {
			get{ return( Get( () => SelectedArtist )); }
			set {
				Set( () => SelectedArtist, value );

				if( value != null ) {
					mEventAggregator.Publish( new Events.ArtistFocusRequested( value.DbId ));
				}
			}
		}

		public string FilterText {
			get { return( Get( () => FilterText )); }
			set {
				Execute.OnUIThread( () => {
					Set( () => FilterText, value );

					if( mArtistView != null ) {
						mArtistView.Refresh();

						// If we have filtered down to one artist, just select it.
						if(( mArtistView is CollectionView ) &&
						  (( mArtistView as CollectionView ).Count == 1 )) {
							SelectedArtist = mArtistView.OfType<UiArtist>().FirstOrDefault();
						}
					}

					RaisePropertyChanged( () => FilterText );
				});
			}
		}

		public void Execute_ToggleSortDisplay() {
			VisualStateName = VisualStateName == cHideSortDescriptions ? cDisplaySortDescriptionss : cHideSortDescriptions;
		}

		internal TaskHandler ArtistsRetrievalTaskHandler {
			get {
				if( mArtistRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mArtistRetrievalTaskHandler = new TaskHandler());
				}

				return ( mArtistRetrievalTaskHandler );
			}
			set { mArtistRetrievalTaskHandler = value; }
		}

		private void RetrieveArtists() {
			ArtistsRetrievalTaskHandler.StartTask( () => {
				using( var artists = mArtistProvider.GetArtistList()) {
					SetArtistList( artists.List );
				}
			},
			() => { },
			ex => NoiseLogger.Current.LogException( "ArtistListViewModel:RetrieveArtists", ex ) );
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist();

			if( dbArtist != null ) {
				Mapper.DynamicMap( dbArtist, retValue );
				retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

				if( mEnableSortPrefixes ) {
					FormatSortPrefix( retValue );
				}
			}

			return ( retValue );
		}

		private void FormatSortPrefix( UiArtist artist ) {
			if( mSortPrefixes != null ) {
				foreach( string prefix in mSortPrefixes ) {
					if( artist.Name.StartsWith( prefix, StringComparison.CurrentCultureIgnoreCase )) {
						artist.SortName = artist.Name.Remove( 0, prefix.Length ).Trim();
						artist.DisplayName = "(" + artist.Name.Insert( prefix.Length, ")" );

						break;
					}
				}
			}
		}

		private void SetArtistList( IEnumerable<DbArtist> artistList ) {
			mArtistList.IsNotifying = false;
			mArtistList.Clear();

			foreach( var artist in artistList ) {
				AddArtist( artist );
			}

			mArtistList.IsNotifying = true;
			mArtistList.Refresh();
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
					Mapper.DynamicMap( artist, uiArtist );
					uiArtist.DisplayGenre = mTagManager.GetGenre( artist.Genre );

					if( mEnableSortPrefixes ) {
						FormatSortPrefix( uiArtist );
					}
				}
			}
		}

		private static void OnArtistChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UiBase;

			if( notifier != null ) {
				if( propertyNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( propertyNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}
	}
}
