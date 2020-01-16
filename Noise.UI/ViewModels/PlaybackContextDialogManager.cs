using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ContextType {
		public string	ContextName { get; }
		public bool		IsAlbumContext { get; }

		public ContextType( string name, bool isAlbumContext ) {
			ContextName = name;
			IsAlbumContext = isAlbumContext;
		}
	}

	public class PlaybackContextDialogManager : DialogModelBase {
		private readonly IAudioController		mAudioController;
		private readonly IPlaybackContextWriter	mContextWriter;
		private DbAlbum							mCurrentAlbum;
		private	DbTrack							mCurrentTrack;
		private PlaybackContext					mTrackContext;
		private PlaybackContext					mAlbumContext;
		private bool							mIsAlbumContext;
		private readonly List<ContextType>		mContextTypes;
		private readonly ContextType			mAlbumContextType;
		private readonly ContextType			mTrackContextType;
		private ContextType						mCurrentContext;

        public	IList<ContextType>				ContextTypes => mContextTypes;
        public	ScPlayContext					PlaybackContext => mIsAlbumContext ? mAlbumContext : mTrackContext;
        public	string							ContextDescription => mIsAlbumContext ? mCurrentAlbum.Name : mCurrentTrack.Name;

		public PlaybackContextDialogManager( IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mAudioController = audioController;
			mContextWriter = contextWriter;

			mTrackContextType = new ContextType( "Track", false );
			mAlbumContextType = new ContextType( "Album", true );
			mContextTypes = new List<ContextType>{ mAlbumContextType, mTrackContextType };
		}

		public void SetTrack( DbAlbum album, DbTrack track ) {
			mCurrentAlbum = album;
			mCurrentTrack = track;

			mTrackContext = mContextWriter.GetTrackContext( mCurrentTrack ) ?? new PlaybackContext();
			mAlbumContext = mContextWriter.GetAlbumContext( mCurrentTrack ) ?? new PlaybackContext();

			CurrentContext = mTrackContext.HasContext() ? mTrackContextType : mAlbumContext.HasContext() ? mAlbumContextType : mTrackContextType;
		}

		public void UpdatePlaybackContext() {
			if( mIsAlbumContext ) {
				mAlbumContext.ReadContext( mAudioController );

				mContextWriter.SaveAlbumContext( mCurrentTrack, mAlbumContext );
			}
			else {
				mTrackContext.ReadContext( mAudioController );

				mContextWriter.SaveTrackContext( mCurrentTrack, mTrackContext );
			}
		}

        public ContextType CurrentContext {
			get => ( mCurrentContext );
            set {
				mCurrentContext = value;
				mIsAlbumContext = mCurrentContext.IsAlbumContext;
				
				RaisePropertyChanged( () => CurrentContext );
				RaisePropertyChanged( () => PlaybackContext );
				RaisePropertyChanged( () => ContextDescription );
			}
		}
    }
}
