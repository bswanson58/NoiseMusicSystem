using System;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessViewModel : AutomaticCommandBase {
        private readonly IProcessManager    mProcessManager;
        private IDisposable                 mProcessingItemChangedSubscription;

        public  BindableCollection<DisplayedProcessItem> ProcessItems { get; }

        public ProcessViewModel( IProcessManager processManager ) {
            mProcessManager = processManager;

            ProcessItems = new BindableCollection<DisplayedProcessItem>();

            mProcessingItemChangedSubscription = mProcessManager.OnProcessingItemChanged.Subscribe( OnProcessingItemEvent );
        }

        private void OnProcessingItemEvent( ProcessItemEvent itemEvent ) {
            switch( itemEvent.Reason ) {
                case EventReason.Add:
                    ProcessItems.Add( new DisplayedProcessItem( itemEvent.Item ));
                    break;

                case EventReason.Completed:
                    DeleteItem( itemEvent.Item );
                    break;
            }
        }

        private void DeleteItem( ProcessItem item ) {
            var displayItem = ProcessItems.FirstOrDefault( i => i.Key.Equals( item.Key ));

            if( displayItem != null ) {
                ProcessItems.Remove( displayItem );
            }
        }

        public void Execute_StartProcessing() {
            mProcessManager.StartProcessing();
        }
    }
}
