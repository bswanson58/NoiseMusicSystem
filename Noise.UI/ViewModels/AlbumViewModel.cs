using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Observal;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : ViewModelBase {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private UiAlbum						mCurrentAlbum;
		public	TimeSpan					AlbumPlayTime { get; private set; }
		private readonly Observer			mChangeObserver;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<UiTrack>	mTracks;

		public AlbumViewModel() {
			mTracks = new ObservableCollectionEx<UiTrack>();

			mChangeObserver = new Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveAlbumInfo( args.Argument as UiAlbum );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SupportInfo = result.Result as AlbumSupportInfo;
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

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum( null, null );

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.DisplayGenre = mNoiseManager.TagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null  );

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		private UiAlbum CurrentAlbum {
			get{ return( mCurrentAlbum ); }
			set {
				Invoke( () => {
					if( mCurrentAlbum != null ) {
						mChangeObserver.Release( mCurrentAlbum );
					}

		        	mCurrentAlbum = value != null ? TransformAlbum( mNoiseManager.DataProvider.GetAlbum( value.DbId )) : null;

					mTracks.Each( node => mChangeObserver.Release( node ));
					mTracks.Clear();

		        	if( mCurrentAlbum != null ) {
						mChangeObserver.Add( mCurrentAlbum );

						AlbumPlayTime = new TimeSpan();

						using( var tracks = mNoiseManager.DataProvider.GetTrackList( mCurrentAlbum.DbId )) {
							foreach( var dbTrack in tracks.List ) {
								mTracks.Add( TransformTrack( dbTrack ));

								AlbumPlayTime += dbTrack.Duration;
							}
						}

						mTracks.Each( track => mChangeObserver.Add( track ));
					}

					RaisePropertyChanged( () => AlbumPlayTime );
					RaisePropertyChanged( () => Album );
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
			CurrentAlbum = TransformAlbum( album );

			if(( CurrentAlbum != null ) &&
			   (!mBackgroundWorker.IsBusy )) {
				mBackgroundWorker.RunWorkerAsync( CurrentAlbum );
			}
		}

		private void OnTrackPlay( long trackId ) {
			mEvents.GetEvent<Events.TrackPlayRequested>().Publish( mNoiseManager.DataProvider.GetTrack( trackId ));
		}

		private AlbumSupportInfo RetrieveAlbumInfo( UiAlbum album ) {
			return( mNoiseManager.DataProvider.GetAlbumSupportInfo( album.DbId ));
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

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			if(( args.Item is DbTrack ) &&
			   ( CurrentAlbum != null ) &&
			   ((args.Item as DbTrack).Album == CurrentAlbum.DbId )) {
				BeginInvoke( () => {
					var track = ( from UiTrack node in mTracks where node.DbId == args.Item.DbId select node ).FirstOrDefault();

					if( track != null ) {
						switch( args.Change ) {
							case DbItemChanged.Update:
								mTracks[mTracks.IndexOf( track )] = TransformTrack( args.Item as DbTrack );
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
					CurrentAlbum = TransformAlbum( args.Item as DbAlbum );
				} );
			}
		}

		public UiAlbum Album {
			get{ return( CurrentAlbum ); }
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] AlbumCover {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.AlbumCovers != null ) &&
				   ( SupportInfo.AlbumCovers.GetLength( 0 ) > 0 )) {
					var cover = (( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.File select artwork ).FirstOrDefault() ??
					             ( from DbArtwork artwork in SupportInfo.AlbumCovers where artwork.Source == InfoSource.Tag select artwork ).FirstOrDefault()) ??
					            SupportInfo.AlbumCovers[0];

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
		public ObservableCollectionEx<UiTrack> TrackList {
			get{ return( mTracks ); }
		}
	}
}
