using System;
using System.Collections.Specialized;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessRecorderViewModel : PropertyChangeBase {
        private readonly IProcessRecorder       mProcessRecorder;
        private string                          mCurrentVolume;

        public  BindableCollection<string>                  VolumeList => mProcessRecorder.AvailableVolumes;
        public  BindableCollection<CompletedProcessItem>    ProcessList { get; private set; }

        public ProcessRecorderViewModel( IProcessRecorder processRecorder ) {
            mProcessRecorder = processRecorder;

            VolumeList.CollectionChanged += OnVolumeListChanged;
        }

        private void OnVolumeListChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            if(( String.IsNullOrWhiteSpace( CurrentVolume )) &&
               ( VolumeList.Any())) {
                CurrentVolume = VolumeList.LastOrDefault();
            }
        }

        public string CurrentVolume {
            get => mCurrentVolume;
            set {
                mCurrentVolume = value;

                if(!String.IsNullOrWhiteSpace( mCurrentVolume )) {
                    ProcessList = mProcessRecorder.GetItemsForVolume( mCurrentVolume );

                    RaisePropertyChanged( () => ProcessList );
                    RaisePropertyChanged( () => CurrentVolume );
                }
            }
        }
    }
}
