﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessRecorderViewModel : PropertyChangeBase {
        private readonly IProcessRecorder       mProcessRecorder;
        private string                          mCurrentVolume;

        public BindableCollection<string>       VolumeList => mProcessRecorder.AvailableVolumes;
        public ICollectionView                  ProcessList { get; private set; }

        public ProcessRecorderViewModel( IProcessRecorder processRecorder ) {
            mProcessRecorder = processRecorder;

            VolumeList.CollectionChanged += OnVolumeListChanged;
        }

        private void OnVolumeListChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            CurrentVolume = VolumeList.LastOrDefault();
        }

        public string CurrentVolume {
            get => mCurrentVolume;
            set {
                mCurrentVolume = value;

                if(!String.IsNullOrWhiteSpace( mCurrentVolume )) {
                    ProcessList = CollectionViewSource.GetDefaultView( mProcessRecorder.GetItemsForVolume( mCurrentVolume ));
                    ProcessList.SortDescriptions.Clear();
                    ProcessList.SortDescriptions.Add( new SortDescription( "FileName", ListSortDirection.Ascending ));
                }
                else {
                    ProcessList = null;
                }

                RaisePropertyChanged(() => ProcessList );
                RaisePropertyChanged(() => CurrentVolume );
            }
        }
    }
}
