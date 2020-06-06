using System;
using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ContextType {
		public string	ContextName { get; }
		public bool		IsAlbumContext { get; }

		public ContextType( string name, bool isAlbumContext ) {
			ContextName = name;
			IsAlbumContext = isAlbumContext;
		}
	}

	public class PlaybackContextDialogManager : PropertyChangeBase, IDialogAware {
		public	const string					cAlbumParameter = "album";
		public  const string					cTrackParameter = "track";

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
        public	string							ContextDescription => mIsAlbumContext ? mCurrentAlbum?.Name : mCurrentTrack?.Name;

        public  string                          Title { get; }
        public  DelegateCommand                 Ok { get; }
        public  DelegateCommand                 Cancel { get; }
        public  event Action<IDialogResult>     RequestClose;

        public PlaybackContextDialogManager( IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mAudioController = audioController;
			mContextWriter = contextWriter;

			mTrackContextType = new ContextType( "Track", false );
			mAlbumContextType = new ContextType( "Album", true );
			mContextTypes = new List<ContextType>{ mAlbumContextType, mTrackContextType };

			Ok = new DelegateCommand( OnOk );
			Cancel = new DelegateCommand( OnCancel );
			Title = "Playback Context";
		}

        public void OnDialogOpened( IDialogParameters parameters ) {
			mCurrentAlbum = parameters.GetValue<DbAlbum>( cAlbumParameter );
			mCurrentTrack = parameters.GetValue<DbTrack>( cTrackParameter );

			if(( mCurrentTrack != null ) &&
               ( mCurrentAlbum != null )) {
                mTrackContext = mContextWriter.GetTrackContext( mCurrentTrack ) ?? new PlaybackContext();
                mAlbumContext = mContextWriter.GetAlbumContext( mCurrentTrack ) ?? new PlaybackContext();

                CurrentContext = mTrackContext.HasContext() ? mTrackContextType : mAlbumContext.HasContext() ? mAlbumContextType : mTrackContextType;
            }
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

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
			UpdatePlaybackContext();

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
