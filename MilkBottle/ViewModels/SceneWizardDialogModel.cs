using System;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MilkBottle.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Platform;

namespace MilkBottle.ViewModels {
    class SceneWizardDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                        cSceneParameter = "scene";
        public  const string                        cNewSceneCreatedParameter = "newScene";
        public  const string                        cPlaybackParameter = "playback";

        private readonly IDialogService             mDialogService;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPresetListProvider        mListProvider;
        private PresetScene                         mScene;
        private PlaybackEvent                       mPlaybackEvent;
        private string                              mArtistNames;
        private string                              mAlbumNames;
        private string                              mTrackNames;
        private string                              mGenres;
        private string                              mTags;
        private string                              mYears;
        private bool                                mUtilizeArtist;
        private bool                                mUtilizeAlbum;
        private bool                                mUtilizeTrack;
        private bool                                mUtilizeGenre;
        private bool                                mUtilizeTags;
        private bool                                mUtilizeYears;
        private UiSource                            mCurrentSource;
        private UiCycling                           mCurrentCycling;
        private PresetList                          mCurrentList;
        private Preset                              mCurrentPreset;
        private int                                 mCurrentCycleDuration;
        private int                                 mCurrentPresetOverlap;
        private bool                                mNewSceneCreated;

        public  string                              Title { get; }

        public  ObservableCollection<UiSource>      SceneSources { get; }
        public  ObservableCollection<UiCycling>     PresetCycling { get; }
        public  ObservableCollection<PresetList>    PresetLists { get; }

        public  DelegateCommand                     CreateScene { get; }
        public  DelegateCommand                     SelectPreset { get; }
        public  DelegateCommand                     SelectScene { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }

        public  string                              PlayingArtist => mPlaybackEvent?.ArtistName;
        public  string                              PlayingAlbum => mPlaybackEvent?.AlbumName;
        public  string                              PlayingTrack => mPlaybackEvent?.TrackName;
        public  string                              PlayingGenre => mPlaybackEvent?.ArtistGenre;
        public  string                              PlayingTags => mPlaybackEvent != null ? String.Join( ", ", mPlaybackEvent.TrackTags ) : String.Empty;
        public  string                              PlayingYear => mPlaybackEvent != null ? mPlaybackEvent.PublishedYear > 0 ? $"{mPlaybackEvent.PublishedYear:D4}" : String.Empty : String.Empty;

        public  string                              SceneName => mScene?.Name;
        public  string                              CurrentPresetName => mCurrentPreset?.Name;
        public  bool                                IsPresetSource => mCurrentSource?.Source == SceneSource.SinglePreset;
        public  bool                                IsListSource => mCurrentSource?.Source == SceneSource.PresetList;

        public  int                                 MinimumCycleDuration { get; private set; }
        public  int                                 MaximumCycleDuration { get; private set; }
        public  string                              CycleDurationLegend => mCurrentCycling?.Cycling == Entities.PresetCycling.Duration ? 
                                                                                                $"{mCurrentCycleDuration} seconds per preset" : 
                                                                                                $"{mCurrentCycleDuration} presets per track";
        public  bool                                CanCycle => IsListSource;

        public  int                                 MinimumPresetOverlap => 0;
        public  int                                 MaximumPresetOverlap => 5;
        public  string                              PresetOverlapLegend => mCurrentPresetOverlap > 0 ? $"{mCurrentPresetOverlap} seconds" : "No Overlap";
        public  bool                                CanOverlap => IsListSource;

        public  event Action<IDialogResult>         RequestClose;

        public SceneWizardDialogModel( IPresetProvider presetProvider, IPresetListProvider listProvider, IDialogService dialogService ) {
            mListProvider = listProvider;
            mPresetProvider = presetProvider;
            mDialogService = dialogService;

            Title = "Scene Wizard";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
            CreateScene = new DelegateCommand( OnCreateScene );
            SelectPreset = new DelegateCommand( OnSelectPreset );
            SelectScene = new DelegateCommand( OnSelectScene );

            PresetLists = new ObservableCollection<PresetList>();
            SceneSources = new ObservableCollection<UiSource> {
                new UiSource( "Preset List", SceneSource.PresetList ),
                new UiSource( "Single Preset", SceneSource.SinglePreset )
            };
            PresetCycling = new ObservableCollection<UiCycling> {
                new UiCycling( "Count", Entities.PresetCycling.CountPerScene ),
                new UiCycling( "Duration", Entities.PresetCycling.Duration )
            };

            mNewSceneCreated = false;
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            if( parameters != null ) {
                mScene = parameters.GetValue<PresetScene>( cSceneParameter );
                mPlaybackEvent = parameters.GetValue<PlaybackEvent>( cPlaybackParameter );

                LoadPresetLists();
                LoadPlaybackEvent();
                LoadScene();
            }
        }

        public void OnCreateScene() {
            mDialogService.ShowDialog( nameof( NewSceneDialog ), new DialogParameters(), OnCreateSceneResult );
        }

        private void OnCreateSceneResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var sceneName = result.Parameters.GetValue<string>( NewSceneDialogModel.cSceneNameParameter );

                if(!String.IsNullOrWhiteSpace( sceneName )) {
                    mScene = new PresetScene( sceneName );
                    mNewSceneCreated = true;

                    UpdateScene();
                    RaisePropertyChanged( () => SceneName );
                }
            }
        }

        public UiSource SelectedSource {
            get => mCurrentSource;
            set {
                mCurrentSource = value;

                OnSceneSourceChanged();
                RaisePropertyChanged( () => SelectedSource );
                RaisePropertyChanged( () => IsListSource );
                RaisePropertyChanged( () => IsPresetSource );
            }
        }

        public PresetList SelectedList {
            get => mCurrentList;
            set {
                mCurrentList = value;

                OnSceneSourceChanged();
                RaisePropertyChanged( () => SelectedList );
            }
        }

        private void OnSceneSourceChanged() {
            if(( mCurrentSource?.Source == SceneSource.PresetList ) &&
               ( mCurrentList != null )) {
                mScene = mScene.WithSource( SceneSource.PresetList, mCurrentList.ListType, mCurrentList.ListIdentifier );
            }

            if(( mCurrentSource?.Source == SceneSource.SinglePreset ) &&
               ( mCurrentPreset != null )) {
                mScene = mScene.WithSource( SceneSource.SinglePreset, PresetListType.Preset, mCurrentPreset.Id );
            }

            RaisePropertyChanged( () => CanCycle );
            RaisePropertyChanged( () => CanOverlap );
        }

        private void OnSelectPreset() {
            mDialogService.ShowDialog( nameof( SelectPresetDialog ), new DialogParameters(), OnPresetSelected );
        }

        private void OnPresetSelected( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                mCurrentPreset = result.Parameters.GetValue<Preset>( SelectPresetDialogModel.cPresetParameter );

                if( mCurrentPreset != null ) {
                    mScene = mScene.WithSource( SceneSource.SinglePreset, PresetListType.Preset, mCurrentPreset.Id );

                    RaisePropertyChanged( () => CurrentPresetName );
                }
            }
        }

        private void OnSelectScene() {
            mDialogService.ShowDialog( nameof( SelectSceneDialog ), new DialogParameters(), OnSelectSceneResult );
        }

        private void OnSelectSceneResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newScene = result.Parameters.GetValue<PresetScene>( SelectSceneDialogModel.cSceneParameter );

                if(newScene != null ) {
                    mScene = newScene;

                    LoadScene();
                }
            }
        }

        public UiCycling CurrentCycling {
            get => mCurrentCycling;
            set {
                mCurrentCycling = value;

                OnPresetCyclingChanged();
                RaisePropertyChanged( () => PresetCycling );
            }
        }

        public int CurrentCycleDuration {
            get => mCurrentCycleDuration;
            set {
                mCurrentCycleDuration = value;

                OnPresetCyclingChanged();
                RaisePropertyChanged( () => CurrentCycleDuration );
            }
        }

        private void OnPresetCyclingChanged() {
            UpdateCycling();

            mScene = mScene.WithCycle( mCurrentCycling.Cycling, mCurrentCycleDuration );
        }

        public int CurrentPresetOverlap {
            get => mCurrentPresetOverlap;
            set {
                mCurrentPresetOverlap = value;

                OnPresetOverlapChanged();
                RaisePropertyChanged( () => CurrentPresetOverlap );
                RaisePropertyChanged( () => PresetOverlapLegend );
            }
        }

        private void OnPresetOverlapChanged() {
            mScene = mScene.WithOverlap( mCurrentPresetOverlap != 0, mCurrentPresetOverlap );
        }

        public string ArtistNames {
            get => mArtistNames;
            set {
                mArtistNames = value;
                mScene = mScene.WithArtists( mArtistNames );

                RaisePropertyChanged( () => ArtistNames );
            }
        }

        public string AlbumNames {
            get => mAlbumNames;
            set {
                mAlbumNames = value;
                mScene = mScene.WithAlbums( mAlbumNames );

                RaisePropertyChanged( () => AlbumNames );
            }
        }

        public string TrackNames {
            get => mTrackNames;
            set {
                mTrackNames = value;
                mScene = mScene.WithTracks( mTrackNames );

                RaisePropertyChanged( () => TrackNames );
            }
        }

        public string Genres {
            get => mGenres;
            set {
                mGenres = value;
                mScene = mScene.WithGenres( mGenres );

                RaisePropertyChanged( () => Genres );
            }
        }

        public string Tags {
            get => mTags;
            set {
                mTags = value;
                mScene = mScene.WithTags( mTags );

                RaisePropertyChanged( () => Tags );
            }
        }

        public string Years {
            get => mYears;
            set {
                mYears = value;
                mScene = mScene.WithYears( mYears );

                RaisePropertyChanged( () => Years );
            }
        }

        public bool UtilizeArtist {
            get => mUtilizeArtist;
            set {
                mUtilizeArtist = value;
                ArtistNames = mUtilizeArtist ? AddText( PlayingArtist, ArtistNames ) : RemoveText( PlayingArtist, ArtistNames );

                RaisePropertyChanged( () => UtilizeArtist );
            }
        }

        public bool UtilizeAlbum {
            get => mUtilizeAlbum;
            set {
                mUtilizeAlbum = value;
                AlbumNames = mUtilizeAlbum ? AddText( PlayingAlbum, AlbumNames ) : RemoveText( PlayingAlbum, AlbumNames );

                RaisePropertyChanged( () => UtilizeAlbum );
            }
        }

        public bool UtilizeTrack {
            get => mUtilizeTrack;
            set {
                mUtilizeTrack = value;
                TrackNames = mUtilizeTrack ? AddText( PlayingTrack, TrackNames ) : RemoveText( PlayingTrack, TrackNames );

                RaisePropertyChanged( () => UtilizeTrack );
            }
        }

        public bool UtilizeGenre {
            get => mUtilizeGenre;
            set {
                mUtilizeGenre = value;
                Genres = mUtilizeGenre ? AddText( PlayingGenre, Genres ) : RemoveText( PlayingGenre, Genres );

                RaisePropertyChanged( () => UtilizeGenre );
            }
        }

        public bool UtilizeTags {
            get => mUtilizeTags;
            set {
                mUtilizeTags = value;
                Tags = mUtilizeTags ? AddText( PlayingTags, Tags ) : RemoveText( PlayingTags, Tags );

                RaisePropertyChanged( () => UtilizeTags );
            }
        }

        public bool UtilizeYears {
            get => mUtilizeYears;
            set {
                mUtilizeYears = value;
                Years = mUtilizeYears ? AddText( PlayingYear, Years ) : RemoveText( PlayingYear, Years );

                RaisePropertyChanged( () => UtilizeYears );
            }
        }

        private string AddText( string text, string toText ) {
            return $"{toText}{(String.IsNullOrWhiteSpace( toText ) ? String.Empty : PresetScene.cValueSeparator.ToString())}{text}";
        }

        private string RemoveText( string text, string fromText ) {
            var newParts = from s in fromText.Split( PresetScene.cValueSeparator ) let trimmed = s.Trim() where !trimmed.Equals( text ) select trimmed;

            return String.Join( PresetScene.cValueSeparator.ToString(), newParts );
        }

        private bool ContainsText( string text, string inText ) {
            var retValue = false;

            if((!String.IsNullOrWhiteSpace( text )) &&
               (!String.IsNullOrWhiteSpace( inText ))) {
                var parts = from s in inText.Split( PresetScene.cValueSeparator ) let trimmed = s.Trim() where trimmed.Equals( text ) select trimmed;

                retValue = parts.Any();
            }

            return retValue;
        }

        private void LoadPresetLists() {
            PresetLists.Clear();
            PresetLists.AddRange( mListProvider.GetLists());
        }

        private void LoadPlaybackEvent() {
            RaisePropertyChanged( () => PlayingArtist );
            RaisePropertyChanged( () => PlayingAlbum );
            RaisePropertyChanged( () => PlayingTrack );
            RaisePropertyChanged( () => PlayingGenre );
            RaisePropertyChanged( () => PlayingTags );
            RaisePropertyChanged( () => PlayingYear );
        }

        private void LoadScene() {
            mArtistNames = mScene.ArtistNames;
            mAlbumNames = mScene.AlbumNames;
            mTrackNames = mScene.TrackNames;
            mGenres = mScene.Genres;
            mTags = mScene.Tags;

            mUtilizeArtist = ContainsText( PlayingArtist, ArtistNames );
            mUtilizeAlbum = ContainsText( PlayingAlbum, AlbumNames );
            mUtilizeTrack = ContainsText( PlayingTrack, TrackNames );
            mUtilizeGenre = ContainsText( PlayingGenre, Genres );
            mUtilizeTags = ContainsText( PlayingTags, Tags );

            mCurrentSource = SceneSources.FirstOrDefault( s => s.Source.Equals( mScene.SceneSource ));
            if( mCurrentSource?.Source == SceneSource.PresetList ) {
                mCurrentList = PresetLists.FirstOrDefault( l => l.ListIdentifier.Equals( mScene.SourceId ));
            }
            else {
                mPresetProvider.GetPresetById( mScene.SourceId ).IfRight( po => po.Do( p => mCurrentPreset = p ));
            }

            mCurrentCycling = PresetCycling.FirstOrDefault( c => c.Cycling.Equals( mScene.PresetCycle ));
            mCurrentCycleDuration = mScene.PresetDuration;

            mCurrentPresetOverlap = mScene.OverlapDuration;

            UpdateCycling();

            RaisePropertyChanged( () => ArtistNames );
            RaisePropertyChanged( () => AlbumNames );
            RaisePropertyChanged( () => TrackNames );
            RaisePropertyChanged( () => Genres );
            RaisePropertyChanged( () => Tags );
            RaisePropertyChanged( () => Years );

            RaisePropertyChanged( () => UtilizeArtist );
            RaisePropertyChanged( () => UtilizeAlbum );
            RaisePropertyChanged( () => UtilizeTrack );
            RaisePropertyChanged( () => UtilizeGenre );
            RaisePropertyChanged( () => UtilizeTags );
            RaisePropertyChanged( () => UtilizeYears );

            RaisePropertyChanged( () => SelectedSource );
            RaisePropertyChanged( () => SelectedList );
            RaisePropertyChanged( () => CurrentPresetName );
            RaisePropertyChanged( () => IsListSource );
            RaisePropertyChanged( () => IsPresetSource );

            RaisePropertyChanged( () => CurrentCycling );
            RaisePropertyChanged( () => CanCycle );
            RaisePropertyChanged( () => CanOverlap );

            RaisePropertyChanged( () => CurrentPresetOverlap );
            RaisePropertyChanged( () => PresetOverlapLegend );

            RaisePropertyChanged( () => SceneName );
        }

        private void UpdateScene() {
            mScene = mScene.WithArtists( mArtistNames );
            mScene = mScene.WithAlbums( mAlbumNames );
            mScene = mScene.WithTracks( mTrackNames );
            mScene = mScene.WithGenres( mGenres );
            mScene = mScene.WithTags( mTags );

            if(( mCurrentSource?.Source == SceneSource.PresetList ) &&
               ( mCurrentList != null )) {
                mScene = mScene.WithSource( SceneSource.PresetList, mCurrentList.ListType, mCurrentList.ListIdentifier );
            }

            if(( mCurrentSource?.Source == SceneSource.SinglePreset ) &&
               ( mCurrentPreset != null )) {
                mScene = mScene.WithSource( SceneSource.SinglePreset, PresetListType.Preset, mCurrentPreset.Id );
            }

            mScene = mScene.WithCycle( mCurrentCycling.Cycling, mCurrentCycleDuration );
            mScene = mScene.WithOverlap( mCurrentPresetOverlap != 0, mCurrentPresetOverlap );
        }

        private void UpdateCycling() {
            MinimumCycleDuration = mCurrentCycling.Cycling == Entities.PresetCycling.CountPerScene ? 1 : PresetDuration.MinimumValue;
            MaximumCycleDuration = mCurrentCycling.Cycling == Entities.PresetCycling.CountPerScene ? 10 : PresetDuration.MaximumValue;

            if( mCurrentCycling.Cycling.Equals( Entities.PresetCycling.CountPerScene  )) {
                mCurrentCycleDuration = Math.Min( mCurrentCycleDuration, 10 );
                mCurrentCycleDuration = Math.Max( mCurrentCycleDuration, 1 );
            }
            else {
                mCurrentCycleDuration = Math.Min( mCurrentCycleDuration, PresetDuration.MaximumValue );
                mCurrentCycleDuration = Math.Max( mCurrentCycleDuration, PresetDuration.MinimumValue );
            }

            RaisePropertyChanged( () => CycleDurationLegend );
            RaisePropertyChanged( () => CurrentCycleDuration );
            RaisePropertyChanged( () => MinimumCycleDuration );
            RaisePropertyChanged( () => MaximumCycleDuration );
        }

        public void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.OK, 
                                        new DialogParameters {{ cSceneParameter, mScene },
                                                              { cNewSceneCreatedParameter, mNewSceneCreated } } ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
