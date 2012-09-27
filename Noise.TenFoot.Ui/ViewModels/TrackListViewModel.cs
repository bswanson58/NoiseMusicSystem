using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TrackListViewModel : BaseListViewModel<DbTrack>, IAlbumTrackList {
		private readonly ITrackProvider		mTrackProvider;
		private long						mCurrentAlbum;
		private TaskHandler					mTrackRetrievalTaskHandler;

		public TrackListViewModel( ITrackProvider trackProvider, IEventAggregator eventAggregator ) :
			base( eventAggregator ) {
			mTrackProvider = trackProvider;
		}

		public void SetContext( long albumId ) {
			if( mCurrentAlbum != albumId ) {
				ItemList.Clear();

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
			                                     		ItemList.AddRange( trackList.List );
			                                     	}
			                                     },
												 () => { SelectedItem = ItemList.FirstOrDefault(); }, 
												 ex => NoiseLogger.Current.LogException( "TrackListViewModel:RetrieveTracksForAlbum", ex )
				);
		}

		protected override void PlayItem() {
			GlobalCommands.PlayTrack.Execute( SelectedItem );
		}
	}
}
