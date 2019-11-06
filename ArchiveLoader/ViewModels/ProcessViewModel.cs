﻿using System;
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

        private void OnProcessingItemEvent( ProcessItemEvent itemEvent ) {
            switch( itemEvent.Reason ) {
                case EventReason.Add:
                    AddItem( itemEvent.Item );
                    break;

                case EventReason.Update:
                    UpdateItemStatus( itemEvent.Item );
                    break;

                case EventReason.Completed:
                    DeleteItem( itemEvent.Item );
                    break;
            }
        }

        private void AddItem( ProcessItem item ) {
            lock( ProcessItems ) {
                ProcessItems.Add( new DisplayedProcessItem( item ));
            }
        }

        private void UpdateItemStatus( ProcessItem item ) {
            var displayItem = ProcessItems.FirstOrDefault( i => i.Key.Equals( item.Key ));

            if( displayItem != null ) {
                var handler = item.ProcessList.FirstOrDefault( i => i.ProcessState == ProcessState.Pending || i.ProcessState == ProcessState.Running );

                displayItem.CurrentHandler = handler != null ? handler.Handler.HandlerName : "Completed";
                displayItem.CurrentState = handler?.ProcessState ?? ProcessState.Completed;
            }
        }

        private void DeleteItem( ProcessItem item ) {
            lock( ProcessItems ) {
                var displayItem = ProcessItems.FirstOrDefault( i => i.Key.Equals( item.Key ));

                if( displayItem != null ) {
                    ProcessItems.Remove( displayItem );
                }
            }
        }

        public void Execute_StartProcessing() {
            mProcessManager.StartProcessing();
        }

        public void Dispose() {
            mProcessManager?.Dispose();
            mProcessingItemChangedSubscription?.Dispose();
        }
    }
}
