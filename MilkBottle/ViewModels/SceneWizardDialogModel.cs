using System;
using System.Linq;
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
        private string                      mArtistNames;
        private string                      mAlbumNames;
        private string                      mTrackNames;
        private string                      mGenres;
        private string                      mTags;
        private bool                        mUtilizeArtist;
        private bool                        mUtilizeAlbum;
        private bool                        mUtilizeTrack;
        private bool                        mUtilizeGenre;
        private bool                        mUtilizeTags;

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

                mArtistNames = mActiveScene.ArtistNames;
                mAlbumNames = mActiveScene.AlbumNames;
                mTrackNames = mActiveScene.TrackNames;
                mGenres = mActiveScene.Genres;
                mTags = mActiveScene.Tags;

                mUtilizeArtist = ContainsText( PlayingArtist, ArtistNames );
                mUtilizeAlbum = ContainsText( PlayingAlbum, AlbumNames );
                mUtilizeTrack = ContainsText( PlayingTrack, TrackNames );
                mUtilizeGenre = ContainsText( PlayingGenre, Genres );
                mUtilizeTags = ContainsText( PlayingTags, Tags );

                RaisePropertyChanged( () => PlayingArtist );
                RaisePropertyChanged( () => PlayingAlbum );
                RaisePropertyChanged( () => PlayingTrack );
                RaisePropertyChanged( () => PlayingGenre );
                RaisePropertyChanged( () => PlayingTags );

                RaisePropertyChanged( () => ArtistNames );
                RaisePropertyChanged( () => AlbumNames );
                RaisePropertyChanged( () => TrackNames );
                RaisePropertyChanged( () => Genres );
                RaisePropertyChanged( () => Tags );

                RaisePropertyChanged( () => UtilizeArtist );
                RaisePropertyChanged( () => UtilizeAlbum );
                RaisePropertyChanged( () => UtilizeTrack );
                RaisePropertyChanged( () => UtilizeGenre );
                RaisePropertyChanged( () => UtilizeTags );
            }
        }

        public string ArtistNames {
            get => mArtistNames;
            set {
                mArtistNames = value;

                OnArtistNamesChanged();
                RaisePropertyChanged( () => ArtistNames );
            }
        }

        private void OnArtistNamesChanged() { }

        public string AlbumNames {
            get => mAlbumNames;
            set {
                mAlbumNames = value;

                OnAlbumNamesChanged();
                RaisePropertyChanged( () => AlbumNames );
            }
        }

        private void OnAlbumNamesChanged() { }

        public string TrackNames {
            get => mTrackNames;
            set {
                mTrackNames = value;

                OnTrackNamesChanged();
                RaisePropertyChanged( () => TrackNames );
            }
        }

        private void OnTrackNamesChanged() { }

        public string Genres {
            get => mGenres;
            set {
                mGenres = value;

                OnGenresChanged();
                RaisePropertyChanged( () => Genres );
            }
        }

        private void OnGenresChanged() { }

        public string Tags {
            get => mTags;
            set {
                mTags = value;

                OnTagsChanged();
                RaisePropertyChanged( () => Tags );
            }
        }

        private void OnTagsChanged() { }

        public bool UtilizeArtist {
            get => mUtilizeArtist;
            set {
                mUtilizeArtist = value;

                OnUtilizeArtist();
                RaisePropertyChanged( () => UtilizeArtist );
            }
        }

        private void OnUtilizeArtist() {
            ArtistNames = mUtilizeArtist ? AddText( PlayingArtist, ArtistNames ) : RemoveText( PlayingArtist, ArtistNames );
        }

        public bool UtilizeAlbum {
            get => mUtilizeAlbum;
            set {
                mUtilizeAlbum = value;

                OnUtilizeAlbum();
                RaisePropertyChanged( () => UtilizeAlbum );
            }
        }

        private void OnUtilizeAlbum() {
            AlbumNames = mUtilizeAlbum ? AddText( PlayingAlbum, AlbumNames ) : RemoveText( PlayingAlbum, AlbumNames );
        }

        public bool UtilizeTrack {
            get => mUtilizeTrack;
            set {
                mUtilizeTrack = value;

                OnUtilizeTrack();
                RaisePropertyChanged( () => UtilizeTrack );
            }
        }

        private void OnUtilizeTrack() {
            TrackNames = mUtilizeTrack ? AddText( PlayingTrack, TrackNames ) : RemoveText( PlayingTrack, TrackNames );
        }

        public bool UtilizeGenre {
            get => mUtilizeGenre;
            set {
                mUtilizeGenre = value;

                OnUtilizeGenre();
                RaisePropertyChanged( () => UtilizeGenre );
            }
        }

        private void OnUtilizeGenre() {
            Genres = mUtilizeGenre ? AddText( PlayingGenre, Genres ) : RemoveText( PlayingGenre, Genres );
        }

        public bool UtilizeTags {
            get => mUtilizeTags;
            set {
                mUtilizeTags = value;

                OnUtilizeTags();
                RaisePropertyChanged( () => UtilizeTags );
            }
        }

        private void OnUtilizeTags() {
            Tags = mUtilizeTags ? AddText( PlayingTags, Tags ) : RemoveText( PlayingTags, Tags );
        }

        private string AddText( string text, string toText ) {
            return $"{toText}{(String.IsNullOrWhiteSpace( toText ) ? String.Empty : ", ")}{text}";
        }

        private string RemoveText( string text, string fromText ) {
            var newParts = from s in fromText.Split( ',' ) let trimmed = s.Trim() where !trimmed.Equals( text ) select trimmed;

            return String.Join( ", ", newParts );
        }

        private bool ContainsText( string text, string inText ) {
            var parts = from s in inText.Split( ',' ) let trimmed = s.Trim() where trimmed.Equals( text ) select trimmed;

            return parts.Any();
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
