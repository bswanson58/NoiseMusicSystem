using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TrackListViewModel : Screen, IAlbumTrackList {
		private readonly ITrackProvider					mTrackProvider;
		private readonly BindableCollection<DbTrack>	mTrackList; 
		private long									mCurrentAlbum;
		private DbTrack									mCurrentTrack;
		private TaskHandler								mTrackRetrievalTaskHandler;

		public TrackListViewModel( ITrackProvider trackProvider ) {
			mTrackProvider = trackProvider;

			mTrackList = new BindableCollection<DbTrack>();
		}

		public void SetContext( long albumId ) {
			if( mCurrentAlbum != albumId ) {
				mTrackList.Clear();

				mCurrentAlbum = albumId;
				RetrieveTracksForAlbum( mCurrentAlbum );
			}
		}

		internal TaskHandler TrackRetrievalTaskHandler {
			get {
				if( mTrackRetrievalTaskHandler == null ) {
					mTrackRetrievalTaskHandler = new TaskHandler();
				}

				return( mTrackRetrievalTaskHandler );
			}

			set{ mTrackRetrievalTaskHandler = value; }
		}

		private void RetrieveTracksForAlbum( long albumId ) {
			TrackRetrievalTaskHandler.StartTask( () => {
			                                     	using( var trackList = mTrackProvider.GetTrackList( albumId )) {
			                                     		mTrackList.AddRange( trackList.List );
			                                     	}
			                                     },
												 () => { }, 
												 ex => NoiseLogger.Current.LogException( "TrackListViewModel:RetrieveTracksForAlbum", ex )
				);
		}

		public BindableCollection<DbTrack> TrackList {
			get{ return( mTrackList ); }
		}

		public DbTrack SelectedTrackList {
			get{ return( mCurrentTrack ); }
			set {
				mCurrentTrack = value;
			}
		}


		public void Home() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateHome();
			}
		}

		public void Done() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateReturn( this, true );
			}
		}
	}
}
