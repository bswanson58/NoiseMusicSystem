using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessRecorderViewModel : PropertyChangeBase {
        private readonly IProcessRecorder       mProcessRecorder;
        private DisplayedStatusItem             mCurrentVolume;
        private bool                            mDisplayOnlyErrors;

        public BindableCollection<DisplayedStatusItem>  VolumeList => mProcessRecorder.AvailableVolumes;
        public ICollectionView                          ProcessList { get; private set; }
        public CompletedProcessItem                     LastItemAdded { get; private set; }

        public ProcessRecorderViewModel( IProcessRecorder processRecorder ) {
            mProcessRecorder = processRecorder;

            VolumeList.CollectionChanged += OnVolumeListChanged;
        }

        private void OnVolumeListChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            CurrentVolume = VolumeList.LastOrDefault();
        }

        public DisplayedStatusItem CurrentVolume {
            get => mCurrentVolume;
            set {
                mCurrentVolume = value;

                if( mCurrentVolume != null ) {
                    if( ProcessList != null ) {
                        ProcessList.CollectionChanged -= OnListChanged;
                    }

                    ProcessList = CollectionViewSource.GetDefaultView( mProcessRecorder.GetItemsForVolume( mCurrentVolume.Name ));
                    ProcessList.SortDescriptions.Clear();
                    ProcessList.SortDescriptions.Add( new SortDescription( "FileName", ListSortDirection.Ascending ));

                    ProcessList.CollectionChanged += OnListChanged;
                    ProcessList.Filter += OnProcessListFilter;
                }
                else {
                    ProcessList = null;
                }

                RaisePropertyChanged(() => ProcessList );
                RaisePropertyChanged(() => CurrentVolume );
            }
        }

        public bool DisplayOnlyErrors {
            get => mDisplayOnlyErrors;
            set {
                mDisplayOnlyErrors = value;

                ProcessList.Refresh();
            }

        }

        private bool OnProcessListFilter( object o ) {
            var retValue = !mDisplayOnlyErrors;

            if(( mDisplayOnlyErrors ) &&
               ( o is CompletedProcessItem item )) {
                if( item.FinalState == ProcessState.Aborted || item.FinalState == ProcessState.Error ) {
                    retValue = true;
                }
            }

            return retValue;
        }

        private void OnListChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            if( args?.NewItems?.Count > 0 ) {
                LastItemAdded = args.NewItems[0] as CompletedProcessItem;

                RaisePropertyChanged( () => LastItemAdded );
            }
        }
    }
}
