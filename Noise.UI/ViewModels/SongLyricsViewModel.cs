using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class UiLyricSelector {
		public	DbLyric		Lyric { get; private set; }
		public	string		ArtistName { get; private set; }
		public	string		SongName { get; private set; }

		public UiLyricSelector( DbLyric lyric, string artistName, string songName ) {
			Lyric = lyric;
			ArtistName = artistName;
			SongName = songName;
		}
	}

	public class SongLyricsViewModel : ViewModelBase {
		private const string		cViewStateClosed	= "Closed";
		private const string		cViewStateNormal	= "Normal";

		private readonly IEventAggregator	mEvents;
		private readonly IArtistProvider	mArtistProvider;
		private readonly ILyricProvider		mLyricProvider;
		private readonly IDialogService		mDialogService;
		private LyricsInfo					mLyricsInfo;
		private readonly ObservableCollectionEx<UiLyricSelector>	mLyricsList;

		public SongLyricsViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, ILyricProvider lyricProvider, IDialogService dialogService  ) {
			mEvents = eventAggregator;
			mArtistProvider = artistProvider;
			mLyricProvider = lyricProvider;
			mDialogService = dialogService;

			mEvents.GetEvent<Events.SongLyricsRequest>().Subscribe( OnLyricsRequest );
			mEvents.GetEvent<Events.PlaybackTrackChanged>().Subscribe( OnTrackChanged );
			mEvents.GetEvent<Events.BalloonPopupOpened>().Subscribe( OnPopupOpened );

			VisualStateName = cViewStateClosed;

			mLyricsList = new ObservableCollectionEx<UiLyricSelector>();
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

		public ObservableCollectionEx<UiLyricSelector> PossibleLyrics {
			get{ return( mLyricsList ); }
		}

		public UiLyricSelector SelectedLyric {
			get{ return( Get( () => SelectedLyric )); }
			set {
				Set( () => SelectedLyric, value );

				if( value != null ) {
					SetCurrentLyric( value.Lyric );
				}
				RaiseCanExecuteChangedEvent( "CanExecute_SelectLyric" );
			}
		}

		private void OnLyricsRequest( LyricsInfo info ) {
			mLyricsInfo = info;

			if(( mLyricsInfo != null ) &&
			   ( mLyricsInfo.MatchedLyric != null )) {
				SetCurrentLyric( mLyricsInfo.MatchedLyric );
				BuildLyricsList();

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

		private void BuildLyricsList() {
			mLyricsList.SuspendNotification();
			mLyricsList.Clear();

			foreach( var lyric in mLyricsInfo.PossibleLyrics ) {
				var artist = mArtistProvider.GetArtist( lyric.ArtistId );

				if( artist != null ) {
					mLyricsList.Add( new UiLyricSelector( lyric, artist.Name, lyric.SongName ));
				}
			}

			mLyricsList.ResumeNotification();
			mLyricsList.Each( item => { if( item.Lyric.DbId == mLyricsInfo.MatchedLyric.DbId ) SelectedLyric = item; } );
		}

		private void OnTrackChanged( object sender ) {
			Close();
		}

		public void Execute_Edit() {
			if( SelectedLyric != null ) {
				using( var updateLyric = mLyricProvider.GetLyricForUpdate( SelectedLyric.Lyric.DbId )) {
					if( mDialogService.ShowDialog( DialogNames.LyricsEdit, updateLyric.Item ) == true ) {
						updateLyric.Update();

						mLyricsInfo.SetMatchingLyric( updateLyric.Item );
						SetCurrentLyric( mLyricsInfo.MatchedLyric );
					}
				}
			}
		}

		[DependsUpon("SelectedLyric")]
		public bool CanExecute_Edit() {
			return( SelectedLyric != null );
		}

		public void Execute_SelectLyric() {
			if(( SelectedLyric != null ) &&
			   ( SelectedLyric.Lyric.TrackId != mLyricsInfo.TrackId )) {
				var newLyric = new DbLyric( mLyricsInfo.ArtistId, mLyricsInfo.TrackId, mLyricsInfo.MatchedLyric.SongName )
												{ Lyrics = SelectedLyric.Lyric.Lyrics, SourceUrl = SelectedLyric.Lyric.SourceUrl };

				mLyricsInfo.SetMatchingLyric( newLyric );
				mLyricProvider.AddLyric( newLyric );

				RaiseCanExecuteChangedEvent( "CanExecute_SelectLyric" );
			}
		}

		public bool CanExecute_SelectLyric() {
			var retValue = false;

			if(( SelectedLyric != null ) &&
			   ( SelectedLyric.Lyric.TrackId != mLyricsInfo.TrackId )) {
				retValue = true;
			}

			return( retValue );
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
