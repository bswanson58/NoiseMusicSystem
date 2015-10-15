using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ContextType {
		public string	ContextName { get; private set; }
		public bool		IsAlbumContext { get; private set; }

		public ContextType( string name, bool isAlbumContext ) {
			ContextName = name;
			IsAlbumContext = isAlbumContext;
		}
	}

	public class PlaybackContextDialogManager : DialogModelBase {
		private readonly IAudioController		mAudioController;
		private readonly IPlaybackContextWriter	mContextWriter;
		private DbAlbum							mCurentAlbum;
		private	DbTrack							mCurrentTrack;
		private PlaybackContext					mTrackContext;
		private PlaybackContext					mAlbumContext;
		private bool							mIsAlbumContext;
		private readonly List<ContextType>		mContextTypes;
		private readonly ContextType			mAlbumContextType;
		private readonly ContextType			mTrackContextType;
		private ContextType						mCurrentContext;

		public PlaybackContextDialogManager( IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mAudioController = audioController;
			mContextWriter = contextWriter;

			mTrackContextType = new ContextType( "Track", false );
			mAlbumContextType = new ContextType( "Album", true );
			mContextTypes = new List<ContextType>{ mAlbumContextType, mTrackContextType };
		}

		public void SetTrack( DbAlbum album, DbTrack track ) {
			mCurentAlbum = album;
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

		public IList<ContextType> ContextTypes {
			get { return(mContextTypes); }
		}

		public ScPlayContext PlaybackContext {
			get { return( mIsAlbumContext ? mAlbumContext : mTrackContext ); }
		}

		public ContextType CurrentContext {
			get { return( mCurrentContext ); }
			set {
				mCurrentContext = value;
				mIsAlbumContext = mCurrentContext.IsAlbumContext;
				
				RaisePropertyChanged( () => CurrentContext );
				RaisePropertyChanged( () => PlaybackContext );
				RaisePropertyChanged( () => ContextDescription );
			}
		}

		public string ContextDescription {
			get {
				return( mIsAlbumContext ? mCurentAlbum.Name : mCurrentTrack.Name );
			}
		}
	}
}
