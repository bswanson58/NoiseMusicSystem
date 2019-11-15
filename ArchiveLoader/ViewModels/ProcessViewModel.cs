using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessViewModel : AutomaticCommandBase, IDisposable {
        private readonly IProcessManager    mProcessManager;
        private readonly IDisposable        mProcessingItemChangedSubscription;
        private readonly BindableCollection<DisplayedProcessItem> mProcessItems;

        public  ICollectionView             ProcessItems { get; }

        public ProcessViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;

            mProcessItems = new BindableCollection<DisplayedProcessItem>();
            ProcessItems = CollectionViewSource.GetDefaultView( mProcessItems );
            ProcessItems.SortDescriptions.Add( new SortDescription( "FileName", ListSortDirection.Ascending ));

            mProcessingItemChangedSubscription = mProcessManager.OnProcessingItemChanged.Subscribe( OnProcessingItemEvent );
        }

        private void OnProcessingItemEvent( Events.ProcessItemEvent itemEvent ) {
            Execute.OnUIThread( () => {
                switch( itemEvent.Reason ) {
                    case CopyProcessEventReason.Add:
                        AddItem( itemEvent.Item );
                        break;

                    case CopyProcessEventReason.Update:
                        UpdateItemStatus( itemEvent.Item );
                        break;

                    case CopyProcessEventReason.Completed:
                        DeleteItem( itemEvent.Item );
                        break;
                }
            });
        }

        private void AddItem( ProcessItem item ) {
            lock( mProcessItems ) {
                mProcessItems.Add( new DisplayedProcessItem( item, OnProcessContinue ));
            }
        }

        private void OnProcessContinue( DisplayedProcessItem item ) {
            mProcessManager.ContinueErroredProcess( item.Key, item.CurrentHandler );
        }

        private void UpdateItemStatus( ProcessItem item ) {
            var displayItem = GetDisplayedItem( item );

            if( displayItem != null ) {
                var handler = item.ProcessList.FirstOrDefault( i => i.ProcessState != ProcessState.Completed );

                displayItem.CurrentHandler = handler != null ? handler.Handler.HandlerName : "Completed";
                displayItem.CurrentState = handler?.ProcessState ?? ProcessState.Completed;

                if( displayItem?.CurrentState == ProcessState.Error ) {
                    displayItem.SetProcessOutput( handler?.ProcessErrOut, handler?.OutputFileCreated == true );
                }
            }
        }

        private void DeleteItem( ProcessItem item ) {
            var displayItem = GetDisplayedItem( item );

            if( displayItem != null ) {
                lock( mProcessItems ) {
                    mProcessItems.Remove( displayItem );
                }
            }
        }

        private DisplayedProcessItem GetDisplayedItem( ProcessItem forItem ) {
            DisplayedProcessItem    retValue;

            lock( mProcessItems ) {
                retValue = mProcessItems.FirstOrDefault( i => i.Key.Equals( forItem.Key ));
            }

            return retValue;
        }

        public void Dispose() {
            mProcessManager?.Dispose();
            mProcessingItemChangedSubscription?.Dispose();
        }
    }
}
