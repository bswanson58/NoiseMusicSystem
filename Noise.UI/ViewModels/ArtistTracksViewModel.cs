using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using ReusableBits;

namespace Noise.UI.ViewModels {
	public class ArtistTracksViewModel : ViewModelBase, IActiveAware,
										 IHandle<Events.DatabaseClosing>,
										 IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly IArtistProvider	mArtistProvider;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly ITrackProvider		mTrackProvider;
		private readonly ITagManager		mTagManager;
		private DbArtist					mCurrentArtist;
		private	bool						mIsActive;
		private TaskHandler<IEnumerable<UiArtistTrackNode>>	mUpdateTask;

		public	event EventHandler			IsActiveChanged;
		public	BindableCollection<UiArtistTrackNode>	TrackList { get; private set; }

		public ArtistTracksViewModel( IEventAggregator eventAggregator,
									  IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mTagManager = tagManager;

			mEventAggregator.Subscribe( this );

			TrackList = new BindableCollection<UiArtistTrackNode>();
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

		public void Handle( Events.DatabaseClosing args ) {
			TrackList.Clear();
			TracksValid = false;
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( mCurrentArtist != null ) {
				if( mCurrentArtist.DbId != request.ArtistId ) {
					UpdateTrackList( request.ArtistId );
				}
			}
			else {
				UpdateTrackList( request.ArtistId );
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			if( mCurrentArtist != null ) {
				if( mCurrentArtist.DbId != request.ArtistId ) {
					UpdateTrackList( request.ArtistId );
				}
			}
			else {
				UpdateTrackList( request.ArtistId );
			}
		}

		private void UpdateTrackList( long artistId ) {
			var artist = mArtistProvider.GetArtist( artistId );

			if( artist != null ) {
				UpdateTrackList( artist );
			}
		}

		internal TaskHandler<IEnumerable<UiArtistTrackNode>>  UpdateTask {
			get {
				if( mUpdateTask == null ) {
					Execute.OnUIThread( () => mUpdateTask = new TaskHandler<IEnumerable<UiArtistTrackNode>>());
				}

				return( mUpdateTask );
			}
			set{ mUpdateTask = value; }
		}

		private void UpdateTrackList( DbArtist artist ) {
			mCurrentArtist = artist;

			TracksValid = false;

			if(( mCurrentArtist != null ) &&
			   ( mIsActive )) {
				UpdateTask.StartTask( () => BuildTrackList( artist ), SetTrackList,
									  ex => NoiseLogger.Current.LogException( "ArtistTracksViewModel:UpdateTrackList", ex ));
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
			TrackList.Clear();
			TrackList.AddRange( from node in list orderby node.Track.Name ascending select node );

			RaisePropertyChanged( () => UniqueTrackCount );
			TracksValid = true;
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
	}
}
