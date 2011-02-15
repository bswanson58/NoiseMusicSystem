using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;

namespace Noise.UI.ViewModels {
	public class ArtistTracksViewModel : ViewModelBase, IActiveAware {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private DbArtist					mCurrentArtist;
		private	bool						mIsActive;
		private readonly BackgroundWorker	mBackgroundWorker;

		public	event EventHandler			IsActiveChanged;
		public	ObservableCollectionEx<UiArtistTrackNode>	TrackList { get; private set; }

		public ArtistTracksViewModel() {
			TrackList = new ObservableCollectionEx<UiArtistTrackNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildTrackList( args.Argument as DbArtist );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetTrackList( result.Result as IEnumerable<UiArtistTrackNode>);

			TracksValid = false;
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
			}
		}

		public bool IsActive {
			get { return( mIsActive ); }
			set {
				mIsActive = value;

				if( mIsActive ) {
					UpdateTrackList( mCurrentArtist );
				}
				else {
					TrackList.Clear();
				}
			}
		}

		public bool TracksValid {
			get{ return( Get( () => TracksValid )); }
			set{ Set( () => TracksValid, value ); }
		}

		private void OnArtistFocus( DbArtist artist ) {
			if(( mCurrentArtist != null ) &&
			   ( artist != null )) {
				if( mCurrentArtist.DbId != artist.DbId ) {
					UpdateTrackList( artist );
				}
			}
			else {
				UpdateTrackList( artist );
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			if( mCurrentArtist != null ) {
				if( mCurrentArtist.DbId != album.Artist ) {
					UpdateTrackList( mNoiseManager.DataProvider.GetArtist( album.Artist ));
				}
			}
			else {
				UpdateTrackList( mNoiseManager.DataProvider.GetArtist( album.Artist ));
			}
		}

		private void UpdateTrackList( DbArtist artist ) {
			mCurrentArtist = artist;

			TracksValid = false;

			if(( mCurrentArtist != null ) &&
			  (!mBackgroundWorker.IsBusy ) &&
			   ( mIsActive )) {
				mBackgroundWorker.RunWorkerAsync( mCurrentArtist );
			}
		}

		public int UniqueTrackCount {
			get{ return( TrackList.Count ); }
		}

		public int AlbumCount {
			get{ return( Get( () => AlbumCount )); }
			set{ Set( () => AlbumCount, value ); }
		}

		private IEnumerable<UiArtistTrackNode> BuildTrackList( DbArtist forArtist ) {
			var trackSet = new Dictionary<string, UiArtistTrackNode>();
			int	albumCount = 0;

			using( var albumList = mNoiseManager.DataProvider.GetAlbumList( forArtist.DbId )) {
				foreach( var album in albumList.List ) {
					using( var trackList = mNoiseManager.DataProvider.GetTrackList( album.DbId )) {
						foreach( var track in trackList.List ) {
							var item = new UiArtistTrackNode( TransformTrack( track ), TransformAlbum( album ));

							if( trackSet.ContainsKey( item.Track.Name )) {
								var parent = trackSet[item.Track.Name];

								if( parent.Children.Count == 0 ) {
									parent.Children.Add( new UiArtistTrackNode( parent.Track, parent.Album ));
								}
								parent.Children.Add( item );
							}
							else {
								item.Level = 1;
								trackSet.Add( item.Track.Name, item );
							}
						}
					}

					albumCount++;
				}
			}

			AlbumCount = albumCount;

			return( trackSet.Values );
		}

		private void SetTrackList( IEnumerable<UiArtistTrackNode> list ) {
			TrackList.SuspendNotification();
			TrackList.Clear();
			TrackList.AddRange( from node in list orderby node.Track.Name ascending select node );
			TrackList.ResumeNotification();

			RaisePropertyChanged( () => UniqueTrackCount );
			TracksValid = true;
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.DisplayGenre = mNoiseManager.TagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null  );

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mNoiseManager.DataProvider.GetTrack( trackId ));
		}

		public void Execute_SwitchView() {
			mEvents.GetEvent<Events.NavigationRequest>().Publish( new NavigationRequestArgs( ViewNames.ArtistTracksView ));
		}
	}
}
