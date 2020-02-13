using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetControlViewModel : PropertyChangeBase, IDisposable, 
                                   IHandle<Events.PresetLibraryUpdated>, IHandle<Events.PresetLibrarySwitched> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IPresetController      mController;
        private readonly IPresetLibrarian       mLibrarian;
        private IDisposable                     mPresetSubscription;
        private string                          mCurrentLibrary;

        public  DelegateCommand                 NextPreset { get; }
        public  DelegateCommand                 PreviousPreset { get; }

        public  string                          PresetName { get; set; }
        public  ObservableCollection<string>    Libraries { get; }

        public PresetControlViewModel( IPresetController presetController, IPresetLibrarian librarian, IEventAggregator eventAggregator ) {
            mController = presetController;
            mLibrarian = librarian;
            mEventAggregator = eventAggregator;

            Libraries = new ObservableCollection<string>();
            mCurrentLibrary = String.Empty;

            NextPreset = new DelegateCommand( OnNextPreset );
            PreviousPreset = new DelegateCommand( OnPreviousPreset );

            mPresetSubscription = mController.CurrentPreset.Subscribe( OnPresetChanged );

            mCurrentLibrary = mController.CurrentPresetLibrary;
            UpdateLibraries();

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.PresetLibraryUpdated args ) {
            UpdateLibraries();
        }

        public void Handle( Events.PresetLibrarySwitched  args ) {
            if( Libraries.Contains( args.LibraryName )) {
                mCurrentLibrary = args.LibraryName;

                RaisePropertyChanged( () => CurrentLibrary );
            }
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
            mEventAggregator.Unsubscribe( this );

            mPresetSubscription?.Dispose();
            mPresetSubscription = null;
        }
    }
}
