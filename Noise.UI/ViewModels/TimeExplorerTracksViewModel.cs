using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using ReusableBits;

namespace Noise.UI.ViewModels {
	public class TimeExplorerTracksViewModel : IHandle<Events.TimeExplorerAlbumFocus>, IHandle<Events.TimeExplorerTrackFocus>,
											   IHandle<Events.DatabaseClosing> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ITrackProvider			mTrackProvider;
		private readonly BindableCollection<UiTrack>	mTrackList; 
		private TaskHandler						mTrackLoaderTaskHandler;

		public TimeExplorerTracksViewModel( IEventAggregator eventAggregator, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mTrackProvider = trackProvider;

			mTrackList = new BindableCollection<UiTrack>();

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.DatabaseClosing args ) {
			mTrackList.Clear();
		}

		public void Handle( Events.TimeExplorerAlbumFocus args ) {
			mTrackList.Clear();
		}

		public void Handle( Events.TimeExplorerTrackFocus args ) {
			LoadTracks( args.AlbumId );
		}

		public BindableCollection<UiTrack> TrackList {
			get{ return( mTrackList ); }
		} 

		internal TaskHandler TrackLoaderTask {
			get {
				if( mTrackLoaderTaskHandler == null ) {
					Execute.OnUIThread( () => mTrackLoaderTaskHandler = new TaskHandler());
				}

				return( mTrackLoaderTaskHandler );
			}

			set{ mTrackLoaderTaskHandler = value; }
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, null  );

			if( dbTrack != null ) {
				Mapper.DynamicMap( dbTrack, retValue );
			}

			return( retValue );
		}

		private void OnTrackPlay( long trackId ) {
			var track = mTrackProvider.GetTrack( trackId );

			if( track != null ) {
				GlobalCommands.PlayTrack.Execute( track );
			}
		}

		private void LoadTracks( long forAlbum ) {
			mTrackList.Clear();

			TrackLoaderTask.StartTask( () => {
						using( var trackList = mTrackProvider.GetTrackList( forAlbum )) {
							foreach( var track in trackList.List ) {
								mTrackList.Add( TransformTrack( track ));
							}
						}
					},
					() => { },
					ex => NoiseLogger.Current.LogException( "TimeExplorerTracksViewModel:LoadTracks", ex ));
		}
	}
}
