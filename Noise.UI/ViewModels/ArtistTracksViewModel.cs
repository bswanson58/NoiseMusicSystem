﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistTracksViewModel : ViewModelBase, IActiveAware {
		private readonly IEventAggregator	mEvents;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly ITagManager		mTagManager;
		private DbArtist					mCurrentArtist;
		private	bool						mIsActive;
		private readonly BackgroundWorker	mBackgroundWorker;

		public	event EventHandler			IsActiveChanged;
		public	ObservableCollectionEx<UiArtistTrackNode>	TrackList { get; private set; }

		public ArtistTracksViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ITagManager tagManager ) {
			mEvents = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTagManager = tagManager;

			mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
			mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );

			TrackList = new ObservableCollectionEx<UiArtistTrackNode>();

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += ( o, args ) => args.Result = BuildTrackList( args.Argument as DbArtist );
			mBackgroundWorker.RunWorkerCompleted += ( o, result ) => SetTrackList( result.Result as IEnumerable<UiArtistTrackNode>);

			TracksValid = false;
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
					UpdateTrackList( mArtistProvider.GetArtist( album.Artist ));
				}
			}
			else {
				UpdateTrackList( mArtistProvider.GetArtist( album.Artist ));
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

			using( var albumList = mAlbumProvider.GetAlbumList( forArtist.DbId )) {
				foreach( var album in albumList.List ) {
					using( var trackList = mTrackProvider.GetTrackList( album.DbId )) {
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
			Execute.OnUIThread( () => { 
				TrackList.SuspendNotification();
				TrackList.Clear();
				TrackList.AddRange( from node in list orderby node.Track.Name ascending select node );
				TrackList.ResumeNotification();

				RaisePropertyChanged( () => UniqueTrackCount );
				TracksValid = true;
			});
		}

		private UiAlbum TransformAlbum( DbAlbum dbAlbum ) {
			var retValue = new UiAlbum();

			Mapper.DynamicMap( dbAlbum, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbAlbum.Genre );

			return( retValue );
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null );

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mTrackProvider.GetTrack( trackId ));
		}

		public void Execute_SwitchView() {
			mEvents.GetEvent<Events.NavigationRequest>().Publish( new NavigationRequestArgs( ViewNames.ArtistTracksView ));
		}
	}
}
