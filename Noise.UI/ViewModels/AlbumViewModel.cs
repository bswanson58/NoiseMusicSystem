using System.Collections.Generic;
using System.ComponentModel;
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
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;
		private DbAlbum				mCurrentAlbum;
		private BackgroundWorker	mBackgroundWorker;
		private readonly Observer	mChangeObserver;
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

				mBackgroundWorker = new BackgroundWorker();
				mBackgroundWorker.DoWork += ( o, args ) => args.Result = mNoiseManager.DataProvider.GetAlbumSupportInfo( args.Argument as DbAlbum );
				mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SupportInfo = result.Result as AlbumSupportInfo;

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
				mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
			if( mCurrentAlbum != null ) {
				if( mCurrentAlbum.Artist != mNoiseManager.DataProvider.GetObjectIdentifier( artist )) {
					SupportInfo = null;
				}
			}
		}

		private void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			var notifier = propertyNotification.Source as UserSettingsNotifier;

			if( notifier != null ) {
				var track = notifier.TargetItem as DbTrack;

				if( track != null ) {
					if( propertyNotification.PropertyName == "UiRating" ) {
						mNoiseManager.DataProvider.SetRating( track, notifier.UiRating );
					}
					if( propertyNotification.PropertyName == "UiIsFavorite" ) {
						mNoiseManager.DataProvider.SetFavorite( track, notifier.UiIsFavorite );
					}
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if( args.Item is DbTrack ) {
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
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
				Invoke( () => {
					Set( () => SupportInfo, value );
					mTracks.Clear();
					mTracks.Each( node => mChangeObserver.Release( node ));

					using( var tracks = mNoiseManager.DataProvider.GetTrackList( mCurrentAlbum )) {
						mTracks.AddRange( from track in tracks.List select new TrackViewNode( mEvents, track ));
					}

					mTracks.Each( track => mChangeObserver.Add( track.UiEdit ));
				});
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			mCurrentAlbum = album;

			if( mCurrentAlbum != null ) {
				if(!mBackgroundWorker.IsBusy ) {
					mBackgroundWorker.RunWorkerAsync( mCurrentAlbum );
				}
			}

			RaisePropertyChanged( "Album" );
		}

		public DbAlbum Album {
			get{ return( mCurrentAlbum ); }
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
