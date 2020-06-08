using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class RecentArtistViewModel : AutomaticPropertyBase,
										   IHandle<Events.ArtistPlayed>, IHandle<Events.ArtistViewed>,
										   IHandle<Events.ArtistAdded>, IHandle<Events.ArtistRemoved>,
										   IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private const int								cRecentListLength = 8;

		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly IArtistProvider				mArtistProvider;
		private readonly ITagManager					mTagManager;
		private readonly List<DbArtist>					mArtistList;
		private readonly BindableCollection<UiArtist>	mRecentlyViewed;
		private readonly BindableCollection<UiArtist>	mRecentlyPlayed; 
		private TaskHandler								mArtistRetrievalTaskHandler;
		private bool									mIAskedForArtistFocus;

		public RecentArtistViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo,
									  IArtistProvider artistProvider, ITagManager tagManager, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;

			mArtistList = new List<DbArtist>();
			mRecentlyPlayed = new BindableCollection<UiArtist>();
			mRecentlyViewed = new BindableCollection<UiArtist>();

			mEventAggregator.Subscribe( this );

			if( databaseInfo.IsOpen ) {
				RetrieveArtistList();
			}
		}

		public void Handle( Events.DatabaseOpened args ) {
			RetrieveArtistList();
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearArtistLists();
		}

		public void Handle( Events.ArtistAdded args ) {
			var addedArtist = mArtistProvider.GetArtist( args.ArtistId );

			if( addedArtist != null ) {
				mArtistList.Add( addedArtist );
			}
		}

		public void Handle( Events.ArtistRemoved args ) {
			var currentArtist = ( from artist in mArtistList where artist.DbId == args.ArtistId select artist ).FirstOrDefault();

			if( currentArtist != null ) {
				mArtistList.Remove( currentArtist );
			}

			BuildRecentlyPlayed();
			BuildRecentlyViewed();
		}
		
		public void Handle( Events.ArtistPlayed args ) {
			UpdateArtist( args.ArtistId );
			
			UpdateRecentlyPlayed( args.ArtistId );
		}

		public void Handle( Events.ArtistViewed args ) {
			UpdateArtist( args.ArtistId );

			if(!mIAskedForArtistFocus ) {
				BuildRecentlyViewed();
			}
		}

		private void UpdateArtist( long artistId ) {
			var currentArtist = ( from artist in mArtistList where artist.DbId == artistId select artist ).FirstOrDefault();

			if( currentArtist != null ) {
				mArtistList.Remove( currentArtist );
			}

			var addedArtist = mArtistProvider.GetArtist( artistId );

			if( addedArtist != null ) {
				mArtistList.Add( addedArtist );
			}
		}

		public IEnumerable<UiArtist> RecentlyPlayedArtists {
			get{ return( mRecentlyPlayed ); }
		} 

		public IEnumerable<UiArtist> RecentlyViewedArtists {
			get{ return( mRecentlyViewed ); }
		}

		public UiArtist SelectedArtist {
			get { return ( Get( () => SelectedArtist ) ); }
			set {
				Set( () => SelectedArtist, value );

				if( value != null ) {
					mIAskedForArtistFocus = true;
					mEventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( value.DbId ));
					mIAskedForArtistFocus = false;
				}
			}
		}

		private void ClearArtistLists() {
			mArtistList.Clear();
			mRecentlyPlayed.Clear();
			mRecentlyViewed.Clear();
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

		private void RetrieveArtistList() {
			ArtistsRetrievalTaskHandler.StartTask( () => {
				using( var artists = mArtistProvider.GetArtistList()) {
					if( artists.List != null ) {
						mArtistList.AddRange( artists.List );
					}
				}

				BuildRecentlyPlayed();
				BuildRecentlyViewed();
			},
			() => { },
			ex => mLog.LogException( "Retrieving Artists", ex ) );
		}

		private void BuildRecentlyPlayed() {
			var recentList = ( from artist in mArtistList where artist.LastPlayedTicks > 0 orderby artist.LastPlayedTicks descending select artist ).Take( cRecentListLength ).ToList();

			mRecentlyPlayed.IsNotifying = false;
			mRecentlyPlayed.Clear();

			foreach( var artist in recentList ) {
				mRecentlyPlayed.Add( TransformArtist( artist ));
			}

			mRecentlyPlayed.IsNotifying = true;
			mRecentlyPlayed.Refresh();
		}

		private void UpdateRecentlyPlayed( long artistId ) {
			var skipIt = false;

			// Update the list in a manner that allows the ui to animate the changes.
			if( mRecentlyPlayed.Any()) {
				if( mRecentlyPlayed[0].DbId == artistId ) {
					skipIt = true;
				}
				else {
					var currentArtist = ( from artist in mRecentlyPlayed where artist.DbId == artistId select artist ).FirstOrDefault();

					if( currentArtist != null ) {
						mRecentlyPlayed.Remove( currentArtist );
					}
				}
			}

			if( !skipIt ) {
				var addedArtist = ( from artist in mArtistList where artist.DbId == artistId select artist ).FirstOrDefault();

				if( addedArtist != null ) {
					mRecentlyPlayed.Insert( 0, TransformArtist( addedArtist ));
				}

				while( mRecentlyPlayed.Count > cRecentListLength ) {
					mRecentlyPlayed.RemoveAt( mRecentlyPlayed.Count - 1 );
				}
			}
		}

		private void BuildRecentlyViewed() {
			var recentList = ( from artist in mArtistList where artist.LastViewedTicks > 0 orderby artist.LastViewedTicks descending select artist ).Take( cRecentListLength ).ToList();

			mRecentlyViewed.IsNotifying = false;
			mRecentlyViewed.Clear();

			foreach( var artist in recentList ) {
				mRecentlyViewed.Add( TransformArtist( artist ));
			}

			mRecentlyViewed.IsNotifying = true;
			mRecentlyViewed.Refresh();
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist( dbArtist );

			if( dbArtist != null ) {
				retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );
			}

			return ( retValue );
		}
	}
}
