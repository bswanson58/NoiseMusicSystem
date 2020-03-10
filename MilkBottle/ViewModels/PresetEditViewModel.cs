using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetEditViewModel : PropertyChangeBase, IHandle<Events.ModeChanged>, IHandle<Events.InitializationComplete> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPresetLibraryProvider     mLibraryProvider;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPresetController          mPresetController;
        private readonly IStateManager              mStateManager;
        private readonly ITagProvider               mTagProvider;
        private readonly IDialogService             mDialogService;
        private readonly IPreferences               mPreferences;
        private PresetLibrary                       mCurrentLibrary;
        private Preset                              mCurrentPreset;

        public  BindableCollection<PresetLibrary>   Libraries { get; }
        public  BindableCollection<Preset>          Presets { get; }
        public  BindableCollection<UiTag>           Tags { get; }

        public  DelegateCommand                     NewTag { get; }

        public PresetEditViewModel( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider, ITagProvider tagProvider, IPreferences preferences,
                                    IPresetController presetController,  IStateManager stateManager, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mLibraryProvider = libraryProvider;
            mPresetProvider = presetProvider;
            mPresetController = presetController;
            mStateManager = stateManager;
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mPreferences = preferences;

            Libraries = new BindableCollection<PresetLibrary>();
            Presets = new BindableCollection<Preset>();
            Tags = new BindableCollection<UiTag>();

            NewTag = new DelegateCommand( OnNewTag );

            mPresetController.BlendPresetTransition = false;
            mPresetController.ConfigurePresetSequencer( PresetSequence.Sequential );
            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
            mStateManager.EnterState( eStateTriggers.Stop );

            LoadLibraries();
            LoadTags();

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            mEventAggregator.Subscribe( this );
        }

        private void Initialize() {
            var milkPreferences = mPreferences.Load<MilkPreferences>();

            CurrentLibrary = Libraries.FirstOrDefault( l => l.Name.Equals( milkPreferences.CurrentPresetLibrary ));
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Review ) {
                mEventAggregator.Unsubscribe( this );
            }
        }

        public PresetLibrary CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibraryChanged();
                RaisePropertyChanged( () => CurrentLibrary );

                if( mCurrentLibrary != null ) {
                    var preferences = mPreferences.Load<MilkPreferences>();

                    preferences.CurrentPresetLibrary = mCurrentLibrary.Name;

                    mPreferences.Save( preferences );
                }
            }
        }

        public Preset CurrentPreset {
            get => mCurrentPreset;
            set {
                mCurrentPreset = value;

                OnPresetChanged();
                RaisePropertyChanged( () => CurrentPreset );
            }
        }

        private void OnLibraryChanged() {
            LoadPresets();

            if( CurrentLibrary != null ) {
                mPresetController.LoadLibrary( CurrentLibrary );
            }
        }

        private void OnPresetChanged() {
            SetPresetState();

            if( CurrentPreset != null ) {
                mPresetController.PlayPreset( CurrentPreset );

                mStateManager.EnterState( eStateTriggers.Run );
            }
            else {
                mStateManager.EnterState( eStateTriggers.Stop );
            }
        }

        private void LoadLibraries() {
            Libraries.Clear();

            mLibraryProvider.SelectLibraries( list => Libraries.AddRange( from l in list orderby l.Name select l ));
        }

        private void LoadPresets() {
            var restoreToPreset = CurrentPreset;

            Presets.Clear();

            if( mCurrentLibrary != null ) {
                mPresetProvider.SelectPresets( mCurrentLibrary, list => Presets.AddRange( from p in list orderby p.Name select p ));
            }

            if( restoreToPreset != null ) {
                CurrentPreset = Presets.FirstOrDefault( p => p.Id.Equals( restoreToPreset.Id ));
            }

            if( CurrentPreset == null ) {
                CurrentPreset = Presets.FirstOrDefault();
            }
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagSelected )));
        }

        private void OnTagSelected( UiTag tag ) {
            if( CurrentPreset != null ) {
                var preset = CurrentPreset.WithTagState( tag.Tag, tag.IsSelected );

                mPresetProvider.Update( preset );

                LoadPresets();
            }
        }

        public bool IsFavorite {
            get => CurrentPreset?.IsFavorite ?? false;
            set {
                if( CurrentPreset != null ) {
                    var preset = CurrentPreset.WithFavorite( value );

                    mPresetProvider.Update( preset );

                    LoadPresets();
                }
            }
        }

        public bool DoNotPlay {
            get => CurrentPreset != null && CurrentPreset.Rating == PresetRating.DoNotPlayValue;
            set {
                if( CurrentPreset != null ) {
                    var preset = CurrentPreset.WithRating( value ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue );

                    mPresetProvider.Update( preset );

                    LoadPresets();
                }
            }
        }

        private void SetPresetState() {
            foreach( var tag in Tags ) {
                tag.SetSelectedState( false );
            }

            if( CurrentPreset != null ) {
                foreach( var tag in CurrentPreset.Tags ) {
                    var uiTag = Tags.FirstOrDefault( t => t.Tag.Id.Equals( tag.Id ));

                    uiTag?.SetSelectedState( true );
                }
            }

            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => DoNotPlay );
        }

        private void OnNewTag() {
            mDialogService.ShowDialog( "NewTagDialog", null, OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag );

                LoadTags();
            }
        }
    }
}
