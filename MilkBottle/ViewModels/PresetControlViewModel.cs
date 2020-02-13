using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, IHandle<Events.PresetLibraryUpdated> {
        private readonly IPresetController      mController;
        private readonly IPresetLibrarian       mLibrarian;
        private IDisposable                     mPresetSubscription;
        private string                          mCurrentLibrary;

        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }

        public  string                          PresetName { get; set; }
        public  ObservableCollection<string>    Libraries { get; }

        public PresetControlViewModel( IPresetController presetController, IPresetLibrarian librarian ) {
            mController = presetController;
            mLibrarian = librarian;

            Libraries = new ObservableCollection<string>();
            mCurrentLibrary = String.Empty;

            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );

            mPresetSubscription = mController.CurrentPreset.Subscribe( OnPresetChanged );

            UpdateLibraries();
        }

        public void Handle( Events.PresetLibraryUpdated args ) {
            UpdateLibraries();
        }

        public string CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                if(!String.IsNullOrWhiteSpace( mCurrentLibrary )) {
                    mController.LoadLibrary( mCurrentLibrary );
                }

                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void UpdateLibraries() {
            Libraries.Clear();
            Libraries.AddRange( mLibrarian.AvailableLibraries );

            if(( String.IsNullOrWhiteSpace( mCurrentLibrary )) ||
               (!Libraries.Contains( mCurrentLibrary ))) {
                mCurrentLibrary = Libraries.FirstOrDefault();

                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void OnPresetChanged( MilkDropPreset preset ) {
            PresetName = preset.PresetName;

            RaisePropertyChanged( () => PresetName );
        }

        public bool IsLocked {
            get => !mController.PresetCycling;
            set => mController.PresetCycling = !value;
        }

        public bool IsBlended {
            get => mController.BlendPresetTransition;
            set => mController.BlendPresetTransition = value;
        }

        private void OnNextPreset() {
            mController.SelectNextPreset();
        }

        private void OnPreviousPreset() {
            mController.SelectPreviousPreset();
        }

        public void Dispose() {
            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
