using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Input;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TrackListViewModel : Screen, IAlbumTrackList,
									  IHandle<InputEvent> {
		private readonly ITrackProvider					mTrackProvider;
		private readonly IEventAggregator				mEventAggregator;
		private readonly BindableCollection<DbTrack>	mTrackList; 
		private long									mCurrentAlbum;
		private DbTrack									mCurrentTrack;
		private TaskHandler								mTrackRetrievalTaskHandler;

		public TrackListViewModel( ITrackProvider trackProvider, IEventAggregator eventAggregator ) {
			mTrackProvider = trackProvider;
			mEventAggregator = eventAggregator;

			mTrackList = new BindableCollection<DbTrack>();
		}

		protected override void OnActivate() {
			base.OnActivate();

			mEventAggregator.Subscribe( this );
		}

		protected override void OnDeactivate( bool close ) {
			base.OnDeactivate( close );

			mEventAggregator.Unsubscribe( this );
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
												 () => { SelectedTrackList = mTrackList.FirstOrDefault(); }, 
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

				NotifyOfPropertyChange( () => SelectedTrackList );
			}
		}

		public void Handle( InputEvent input ) {
			switch( input.Command ) {
				case InputCommand.Down:
				case InputCommand.Right:
					SelectNextTrack();
					break;

				case InputCommand.Up:
				case InputCommand.Left:
					SelectPreviousTrack();
					break;

				case InputCommand.Play:
					if( mCurrentTrack != null ) {
						GlobalCommands.PlayTrack.Execute( mCurrentTrack );
					}
					break;

				case InputCommand.Back:
					Done();
					break;

				case InputCommand.Home:
					Home();
					break;
			}
		}

		private void SelectNextTrack() {
			if( mTrackList.Count > 0 ) {
				if( mCurrentTrack != null ) {
					var index = mTrackList.IndexOf( mCurrentTrack );

					if(( index != -1 ) &&
					  (( index + 1 ) < mTrackList.Count )) {
						SelectedTrackList = mTrackList[index + 1];
					}
					else {
						SelectedTrackList = mTrackList[0];
					}
				}
				else {
					SelectedTrackList = mTrackList[0];
				}
			}
		}

		private void SelectPreviousTrack() {
			if( mTrackList.Count > 0 ) {
				if( mCurrentTrack != null ) {
					var index = mTrackList.IndexOf( mCurrentTrack );

					SelectedTrackList = index == 0 ? mTrackList[mTrackList.Count - 1] : mTrackList[index - 1 ];
				}
				else {
					SelectedTrackList = mTrackList[mTrackList.Count - 1];
				}
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
