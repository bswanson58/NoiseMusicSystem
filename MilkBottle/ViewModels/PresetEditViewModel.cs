using System.Linq;
using Caliburn.Micro;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetEditViewModel : PropertyChangeBase {
        private readonly IPresetLibraryProvider     mLibraryProvider;
        private readonly IPresetProvider            mPresetProvider;
        private PresetLibrary                       mCurrentLibrary;
        private Preset                              mCurrentPreset;

        public  BindableCollection<PresetLibrary>   Libraries { get; }
        public  BindableCollection<Preset>          Presets { get; }

        public PresetEditViewModel( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider ) {
            mLibraryProvider = libraryProvider;
            mPresetProvider = presetProvider;

            Libraries = new BindableCollection<PresetLibrary>();
            Presets = new BindableCollection<Preset>();

            LoadLibraries();
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
        }

        private void OnPresetChanged() {
            SetPresetState();
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
