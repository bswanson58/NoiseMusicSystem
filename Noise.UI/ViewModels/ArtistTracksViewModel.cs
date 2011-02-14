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
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
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

		private void UpdateTrackList( DbArtist artist ) {
			mCurrentArtist = artist;

			if(( mCurrentArtist != null ) &&
			  (!mBackgroundWorker.IsBusy ) &&
			   ( mIsActive )) {
				mBackgroundWorker.RunWorkerAsync( mCurrentArtist );
			}
		}

		private IEnumerable<UiArtistTrackNode> BuildTrackList( DbArtist forArtist ) {
			var trackSet = new Dictionary<string, UiArtistTrackNode>();
			using( var albumList = mNoiseManager.DataProvider.GetAlbumList( forArtist.DbId )) {
				foreach( var album in albumList.List ) {
					using( var trackList = mNoiseManager.DataProvider.GetTrackList( album.DbId )) {
						foreach( var track in trackList.List ) {
							var item = new UiArtistTrackNode( TransformTrack( track ), TransformAlbum( album ));

							if( trackSet.ContainsKey( item.Track.Name )) {
								var parent = trackSet[item.Track.Name];

								parent.Children.Add( item );
							}
							else {
								trackSet.Add( item.Track.Name, item );
							}
						}
					}
				}
			}

			return( trackSet.Values );
		}

		private void SetTrackList( IEnumerable<UiArtistTrackNode> list ) {
			TrackList.SuspendNotification();
			TrackList.Clear();
			TrackList.AddRange( from node in list orderby node.Track.Name ascending select node );
			TrackList.ResumeNotification();
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
			
		}

		public void Execute_SwitchView() {
			mEvents.GetEvent<Events.NavigationRequest>().Publish( new NavigationRequestArgs( ViewNames.ArtistTracksView ));
		}
	}
}
