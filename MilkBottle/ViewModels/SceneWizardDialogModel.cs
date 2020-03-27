using System;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class SceneWizardDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                cSceneParameter = "scene";
        public  const string                cPlaybackParameter = "playback";

        private readonly ISceneProvider     mSceneProvider;
        private PresetScene                 mActiveScene;
        private PlaybackEvent               mPlaybackEvent;

        public  String                      Name { get; set; }
        public  string                      Title { get; }

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  string                      PlayingArtist => mPlaybackEvent?.ArtistName;
        public  string                      PlayingAlbum => mPlaybackEvent?.AlbumName;
        public  string                      PlayingTrack => mPlaybackEvent?.TrackName;
        public  string                      PlayingGenre => mPlaybackEvent?.ArtistGenre;
        public  string                      PlayingTags => mPlaybackEvent != null ? String.Join( ", ", mPlaybackEvent.TrackTags ) : String.Empty;

        public  event Action<IDialogResult> RequestClose;

        public SceneWizardDialogModel( ISceneProvider sceneProvider ) {
            mSceneProvider = sceneProvider;

            Title = "Scene Wizard";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnDialogOpened( IDialogParameters parameters ) {
            if( parameters != null ) {
                mActiveScene = parameters.GetValue<PresetScene>( cSceneParameter );
                mPlaybackEvent = parameters.GetValue<PlaybackEvent>( cPlaybackParameter );

                RaisePropertyChanged( () => PlayingArtist );
                RaisePropertyChanged( () => PlayingAlbum );
                RaisePropertyChanged( () => PlayingTrack );
                RaisePropertyChanged( () => PlayingGenre );
                RaisePropertyChanged( () => PlayingTags );
            }
        }

        public void OnOk() {
            RaiseRequestClose(
                !String.IsNullOrWhiteSpace( Name )
                    ? new DialogResult( ButtonResult.OK, new DialogParameters {{ cSceneParameter, mActiveScene }} )
                    : new DialogResult( ButtonResult.Cancel ) );
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
