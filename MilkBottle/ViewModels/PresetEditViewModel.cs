using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetEditViewModel : PropertyChangeBase, IHandle<Events.ModeChanged>, IHandle<Events.InitializationComplete> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPresetLibraryProvider     mLibraryProvider;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPresetController          mPresetController;
        private readonly IStateManager              mStateManager;
        private readonly ITagProvider               mTagProvider;
        private PresetLibrary                       mCurrentLibrary;
        private Preset                              mCurrentPreset;

        public  BindableCollection<PresetLibrary>   Libraries { get; }
        public  BindableCollection<Preset>          Presets { get; }
        public  BindableCollection<UiTag>           Tags { get; }

        public PresetEditViewModel( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider, ITagProvider tagProvider,
                                    IPresetController presetController,  IStateManager stateManager, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mLibraryProvider = libraryProvider;
            mPresetProvider = presetProvider;
            mPresetController = presetController;
            mStateManager = stateManager;
            mTagProvider = tagProvider;

            Libraries = new BindableCollection<PresetLibrary>();
            Presets = new BindableCollection<Preset>();
            Tags = new BindableCollection<UiTag>();

            LoadLibraries();
            LoadTags();

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Review ) {
                mEventAggregator.Unsubscribe( this );
            }
        }

        public void Handle( Events.InitializationComplete args ) {
            mStateManager.EnterState( eStateTriggers.Run );
        }

        public PresetLibrary CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibraryChanged();
                RaisePropertyChanged( () => CurrentLibrary );
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
            }
        }

        private void LoadLibraries() {
            Libraries.Clear();

            mLibraryProvider.SelectLibraries( list => Libraries.AddRange( from l in list orderby l.Name select l ));

            if( CurrentLibrary != null ) {
                CurrentLibrary = Libraries.FirstOrDefault( l => l.Id.Equals( mCurrentLibrary.Id ));
            }

            if( CurrentLibrary == null ) {
                CurrentLibrary = Libraries.FirstOrDefault();
            }
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

        private void OnTagSelected( UiTag tag ) { }

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
            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => DoNotPlay );
        }
    }
}
