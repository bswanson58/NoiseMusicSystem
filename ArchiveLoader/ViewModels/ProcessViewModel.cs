using System;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessViewModel : AutomaticCommandBase, IDisposable {
        private readonly IProcessManager    mProcessManager;
        private readonly IDisposable        mProcessingItemChangedSubscription;

        public  BindableCollection<DisplayedProcessItem> ProcessItems { get; }

        public ProcessViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;

            ProcessItems = new BindableCollection<DisplayedProcessItem>();

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
            lock( ProcessItems ) {
                ProcessItems.Add( new DisplayedProcessItem( item, OnProcessContinue ));
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

                if( displayItem.CurrentState == ProcessState.Error ) {
                    displayItem.SetProcessOutput( handler?.ProcessErrOut );
                }
            }
        }

        private void DeleteItem( ProcessItem item ) {
            var displayItem = GetDisplayedItem( item );

            if( displayItem != null ) {
                lock( ProcessItems ) {
                    ProcessItems.Remove( displayItem );
                }
            }
        }

        private DisplayedProcessItem GetDisplayedItem( ProcessItem forItem ) {
            DisplayedProcessItem    retValue;

            lock( ProcessItems ) {
                retValue = ProcessItems.FirstOrDefault( i => i.Key.Equals( forItem.Key ));
            }

            return retValue;
        }

        public void Dispose() {
            mProcessManager?.Dispose();
            mProcessingItemChangedSubscription?.Dispose();
        }
    }
}
