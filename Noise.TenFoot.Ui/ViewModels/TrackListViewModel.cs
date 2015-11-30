using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TrackListViewModel : BaseListViewModel<DbTrack>, IAlbumTrackList, ITitledScreen {
		private readonly ITrackProvider		mTrackProvider;
		private readonly IPlayCommand		mPlayCommand;
		private readonly IUiLog				mLog;
		private long						mCurrentAlbum;
		private TaskHandler					mTrackRetrievalTaskHandler;

		public	string						ScreenTitle { get; private set; }
		public	string						Context { get; private set; }

		public TrackListViewModel( IEventAggregator eventAggregator, ITrackProvider trackProvider, IPlayCommand playCommand, IUiLog log ) :
			base( eventAggregator ) {
			mTrackProvider = trackProvider;
			mPlayCommand = playCommand;
			mLog = log;

			ScreenTitle = "Tracks";
			Context = "";
		}

		public void SetContext( DbAlbum album ) {
			if(( album != null ) &&
			   ( mCurrentAlbum != album.DbId )) {
				ItemList.Clear();

				mCurrentAlbum = album.DbId;
				RetrieveTracksForAlbum( mCurrentAlbum );

				Context = album.Name;
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
			                                     		ItemList.AddRange( from track in trackList.List 
																		   orderby track.VolumeName, track.TrackNumber ascending
																		   select track );
			                                     	}
			                                     },
												 () => { SelectedItem = ItemList.FirstOrDefault(); }, 
												 ex => mLog.LogException( string.Format( "Retrieving Tracks for Album:{0}", albumId ), ex )
				);
		}

		protected override void DisplayItem() {
			mPlayCommand.Play( SelectedItem );
		}

		protected override void EnqueueItem() {
			mPlayCommand.Play( SelectedItem );
		}

		protected override void DequeueItem() {
			EventAggregator.Publish( new Input.Events.DequeueTrack( SelectedItem ));
		}
	}
}
