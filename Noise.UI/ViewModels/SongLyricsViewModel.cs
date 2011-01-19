using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class SongLyricsViewModel : ViewModelBase {
		private const string		cViewStateClosed	= "Closed";
		private const string		cViewStateNormal	= "Normal";

		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private	INoiseManager		mNoiseManager;
		private LyricsInfo			mLyricsInfo;

		public SongLyricsViewModel() {
			VisualStateName = cViewStateClosed;
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.SongLyricsRequest>().Subscribe( OnLyricsRequest );
				mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnTrackChanged );
				mEvents.GetEvent<Events.BalloonPopupOpened>().Subscribe( OnPopupOpened );
			}
		}

		public string VisualStateName {
			get{ return( Get( () => VisualStateName )); }
			set {
				Set( () => VisualStateName, value );

				if( value == cViewStateNormal ) {
					mEvents.GetEvent<Events.BalloonPopupOpened>().Publish( this );
				}
			}
		}

		public string SongName {
			get{ return( Get( () => SongName )); }
			set{ Set( () => SongName, value ); }
		}

		public string SongLyrics {
			get{ return( Get( () => SongLyrics )); }
			set{ Set( () => SongLyrics, value ); }
		}

		public string LyricsSource {
			get{ return( Get( () => LyricsSource )); }
			set{ Set( () => LyricsSource, value ); }
		}

		private void OnLyricsRequest( LyricsInfo info ) {
			mLyricsInfo = info;

			if(( mLyricsInfo != null ) &&
			   ( mLyricsInfo.MatchedLyric != null )) {
				SetCurrentLyric( mLyricsInfo.MatchedLyric );

				VisualStateName = cViewStateNormal;
			}
			else {
				VisualStateName = cViewStateClosed;
			}
		}

		private void SetCurrentLyric( DbLyric lyric ) {
			SongName = lyric.SongName;
			SongLyrics = lyric.Lyrics;
			LyricsSource = lyric.SourceUrl;
		}

		private void OnTrackChanged( object sender ) {
			Close();
		}

		public void Execute_Edit() {
			var	dialogService = mContainer.Resolve<IDialogService>();
			var updateLyric = mNoiseManager.DataProvider.GetLyricForUpdate( mLyricsInfo.MatchedLyric.DbId );

			if( dialogService.ShowDialog( DialogNames.LyricsEdit, updateLyric.Item ) == true ) {
				updateLyric.Update();

				mLyricsInfo.SetMatchingLyric( updateLyric.Item );
				SetCurrentLyric( mLyricsInfo.MatchedLyric );
			}
		}

		public void Execute_LyricsSourceClicked() {
			mEvents.GetEvent<Events.WebsiteRequest>().Publish( LyricsSource );
		}

		public void Execute_Close() {
			Close();
		}

		private void OnPopupOpened( object sender ) {
			if( sender != this ) {
				Close();
			}
		}

		private void Close() {
			BeginInvoke( () => {
				VisualStateName = cViewStateClosed;
			});
		}
	}
}
