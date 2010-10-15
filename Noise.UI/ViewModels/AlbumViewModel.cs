using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private DbAlbum					mCurrentAlbum;
		public	UserSettingsNotifier	UiEdit { get; private set; }
		public	TimeSpan				AlbumPlayTime { get; private set; }
		private readonly Observer		mChangeObserver;
		private readonly ObservableCollectionEx<TrackViewNode>	mTracks;

		public AlbumViewModel() {
			mTracks = new ObservableCollectionEx<TrackViewNode>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
				mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
			}
		}

		public DbAlbum UiDisplay {
			get{ return( mCurrentAlbum ); }
		}

		private DbAlbum CurrentAlbum {
			get{ return( mCurrentAlbum ); }
			set {
				Invoke( () => {
		        	mCurrentAlbum = value != null ? mNoiseManager.DataProvider.GetAlbum( value.DbId ) : null;

					mTracks.Each( node => mChangeObserver.Release( node ));
					mTracks.Clear();

		        	if( mCurrentAlbum != null ) {
						if( UiEdit != null ) {
							mChangeObserver.Release( UiEdit );
						}
						UiEdit = new UserSettingsNotifier( mCurrentAlbum, null );
						mChangeObserver.Add( UiEdit );

						using( var tracks = mNoiseManager.DataProvider.GetTrackList( CurrentAlbum )) {
							mTracks.AddRange( from track in tracks.List select new TrackViewNode( mEvents, track ));
						}

						AlbumPlayTime = new TimeSpan();
						mTracks.Each( track => AlbumPlayTime += track.Track.Duration );

						mTracks.Each( track => mChangeObserver.Add( track.UiEdit ));
					}

					RaisePropertyChanged( () => AlbumPlayTime );
					RaisePropertyChanged( () => UiDisplay );
					RaisePropertyChanged( () => UiEdit );
		        } );
			}
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
				BeginInvoke( () => Set( () => SupportInfo, value ));
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
			if( CurrentAlbum != null ) {
				if( CurrentAlbum.Artist != artist.DbId ) {
					CurrentAlbum = null;
					SupportInfo = null;
				}
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			CurrentAlbum = album;

			if( CurrentAlbum != null ) {
				SupportInfo = mNoiseManager.DataProvider.GetAlbumSupportInfo( CurrentAlbum );
			}
		}

		private void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				if( notifier.TargetItem is DbTrack ) {
					var track = notifier.TargetItem as DbTrack;

					if( propertyNotification.PropertyName == "UiRating" ) {
						mNoiseManager.DataProvider.SetRating( track, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mNoiseManager.DataProvider.SetFavorite( track, notifier.UiIsFavorite );
					}
				}

				if( notifier.TargetItem is DbAlbum ) {
					var album = notifier.TargetItem as DbAlbum;

					if( propertyNotification.PropertyName == "UiRating" ) {
						mNoiseManager.DataProvider.SetRating( album, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mNoiseManager.DataProvider.SetFavorite( album, notifier.UiIsFavorite );
					}
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if(( args.Item is DbTrack ) &&
			   ( CurrentAlbum != null ) &&
			   ((args.Item as DbTrack).Album == CurrentAlbum.DbId )) {
				BeginInvoke( () => {
					var track = ( from TrackViewNode node in mTracks where node.Track.DbId == args.Item.DbId select node ).FirstOrDefault();

					if( track != null ) {
						switch( args.Change ) {
							case DbItemChanged.Update:
								track.UiDisplay.UpdateObject( args.Item );
								break;
							case DbItemChanged.Delete:
								mTracks.Remove( track );

								break;
						}
					}
				});
			}

			if(( args.Item is DbAlbum ) &&
			   ( CurrentAlbum != null ) &&
			   ((args.Item as DbAlbum).DbId == CurrentAlbum.DbId )) {
				BeginInvoke( () => {
					CurrentAlbum = args.Item as DbAlbum;
				} );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] AlbumCover {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.AlbumCovers != null ) &&
				   ( SupportInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					var cover = ( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault();

					if( cover == null ) {
						cover = ( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault();
					}
					if( cover == null ) {
						cover = SupportInfo.AlbumCovers[0];
					}

					if( cover != null ) {
						retValue = cover.Image;
					}
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public IList<byte[]> AlbumArtwork {
			get {
				List<byte[]>	retValue;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Artwork != null )) {
					retValue = ( from DbArtwork artwork in SupportInfo.Artwork select artwork.Image ).ToList();
				}
				else {
					retValue = new List<byte[]>();
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<TrackViewNode> TrackList {
			get{ return( mTracks ); }
		}
	}
}
