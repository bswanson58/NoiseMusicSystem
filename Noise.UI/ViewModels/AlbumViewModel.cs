using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	internal class NewAlbumInfo {
		public DbAlbum				Album { get; private set; }
		public AlbumSupportInfo		SupportInfo { get; private set; }
		public IEnumerable<DbTrack>	TrackList { get; private set; }

		public NewAlbumInfo() {
		}

		public NewAlbumInfo( DbAlbum album, AlbumSupportInfo supportInfo, IEnumerable<DbTrack> trackList ) {
			Album = album;
			SupportInfo = supportInfo;
			TrackList = trackList;
		}
	}

	internal class AlbumViewModel : ViewModelBase {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private UiAlbum						mCurrentAlbum;
		public	TimeSpan					AlbumPlayTime { get; private set; }
		private readonly Observal.Observer	mChangeObserver;
		private readonly BackgroundWorker	mBackgroundWorker;
		private readonly ObservableCollectionEx<UiTrack>	mTracks;

		public AlbumViewModel() {
			mTracks = new ObservableCollectionEx<UiTrack>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = RetrieveAlbumInfo( args.Argument as DbAlbum );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetCurrentAlbum( result.Result as NewAlbumInfo );
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

		private void SetCurrentAlbum( NewAlbumInfo albumInfo ) {
			Invoke( () => {
				if( mCurrentAlbum != null ) {
					mChangeObserver.Release( mCurrentAlbum );
				}

		        mCurrentAlbum = albumInfo.Album != null ? TransformAlbum( albumInfo.Album ) : null;

				mTracks.Each( node => mChangeObserver.Release( node ));
				mTracks.Clear();

		        if( mCurrentAlbum != null ) {
					mChangeObserver.Add( mCurrentAlbum );

					AlbumPlayTime = new TimeSpan();

					foreach( var dbTrack in albumInfo.TrackList ) {
						mTracks.Add( TransformTrack( dbTrack ));

						AlbumPlayTime += dbTrack.Duration;
					}

					mTracks.Each( track => mChangeObserver.Add( track ));
				}

				SupportInfo = albumInfo.SupportInfo;

				RaisePropertyChanged( () => AlbumPlayTime );
				RaisePropertyChanged( () => Album );
		    } );
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{
				BeginInvoke( () => Set( () => SupportInfo, value ));
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
			if( mCurrentAlbum != null ) {
				if( mCurrentAlbum.Artist != artist.DbId ) {
					SetCurrentAlbum( new NewAlbumInfo());
				}
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			if( album == null ) {
				SetCurrentAlbum( new NewAlbumInfo());
			}
			else {
				if(( mCurrentAlbum == null ) ||
				   ( mCurrentAlbum.DbId != album.DbId )) {
					if(!mBackgroundWorker.IsBusy ) {
						mBackgroundWorker.RunWorkerAsync( album );
					}
				}
			}
		}

		private NewAlbumInfo RetrieveAlbumInfo( DbAlbum album ) {
			var retValue = new NewAlbumInfo( null, null, null );

			if( album != null ) {
				using( var tracks = mNoiseManager.DataProvider.GetTrackList( album.DbId )) {
					var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
														orderby track.VolumeName, track.TrackNumber ascending select track );

					retValue = new NewAlbumInfo( album, mNoiseManager.DataProvider.GetAlbumSupportInfo( album.DbId ), sortedList );
				}
			}

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mNoiseManager.DataProvider.GetTrack( trackId ));
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
			var item = args.GetItem( mNoiseManager.DataProvider );

			if(( item is DbTrack ) &&
			   ( mCurrentAlbum != null ) &&
			   ( args.Change == DbItemChanged.Update ) &&
			   ((item as DbTrack).Album == mCurrentAlbum.DbId )) {
				BeginInvoke( () => {
					var track = ( from UiTrack node in mTracks where node.DbId == args.ItemId select node ).FirstOrDefault();

					if( track != null ) {
						switch( args.Change ) {
							case DbItemChanged.Update:
								var newTrack = TransformTrack( item as DbTrack );

								mChangeObserver.Release( track );
								mTracks[mTracks.IndexOf( track )] = newTrack;
								mChangeObserver.Add( newTrack );

								break;
							case DbItemChanged.Delete:
								mTracks.Remove( track );

								break;
						}
					}
				});
			}

			if(( item is DbAlbum ) &&
			   ( mCurrentAlbum != null ) &&
			   ( args.Change == DbItemChanged.Update ) &&
			   ((item as DbAlbum).DbId == mCurrentAlbum.DbId )) {
				BeginInvoke( () => Mapper.DynamicMap( item as DbAlbum, mCurrentAlbum ));
			}
		}

		public UiAlbum Album {
			get{ return( mCurrentAlbum ); }
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

		public void Execute_PlayAlbum() {
			if( mCurrentAlbum != null ) {
				GlobalCommands.PlayAlbum.Execute( mNoiseManager.DataProvider.GetAlbum( mCurrentAlbum.DbId ));
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
	}
}
