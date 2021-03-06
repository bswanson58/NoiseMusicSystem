﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProcessViewModel : PropertyChangeBase, IDisposable {
        private readonly IProcessManager                mProcessManager;
        private readonly IDisposable                    mProcessingItemChangedSubscription;
        private readonly IPlatformLog                   mLog;
        private bool                                    mHaveErroredProcess;
        private readonly BindableCollection<DisplayedProcessItem> mProcessItems;

        public  ICollectionView                         ProcessItems { get; }

        public  DelegateCommand                         ContinueAll { get; }
        public  DelegateCommand                         AbortAll { get; }

        public ProcessViewModel( IProcessManager processManager, IPlatformLog log ) {
            mProcessManager = processManager;
            mLog = log;

            ContinueAll = new DelegateCommand( OnContinueAll, CanContinueAll );
            AbortAll = new DelegateCommand( OnAbortAll, CanAbortAll );

            mProcessItems = new BindableCollection<DisplayedProcessItem>();
            ProcessItems = CollectionViewSource.GetDefaultView( mProcessItems );
            ProcessItems.SortDescriptions.Add( new SortDescription( "FileName", ListSortDirection.Ascending ));

            mHaveErroredProcess = false;

            mProcessingItemChangedSubscription = mProcessManager.OnProcessingItemChanged.Subscribe( OnProcessingItemEvent );
        }

        private void OnContinueAll() {
            mProcessManager.ContinueAllProcesses();
        }

        private bool CanContinueAll() {
            return mHaveErroredProcess;
        }

        private void OnAbortAll() {
            mProcessManager.AbortAllProcesses();
        }

        private bool CanAbortAll() {
            return mHaveErroredProcess;
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
                mProcessItems.Add( new DisplayedProcessItem( item, OnProcessContinue, OnAbortProcess, OnOpenFolder ));
            }
        }

        private void OnProcessContinue( DisplayedProcessItem item ) {
            mProcessManager.ContinueErroredProcess( item.Key, item.CurrentHandler );
        }

        private void OnAbortProcess( DisplayedProcessItem item ) {
            mProcessManager.AbortErroredProcess( item.Key, item.CurrentHandler );
        }

        private void OnOpenFolder( DisplayedProcessItem item ) {
            try {
                var path = Path.GetDirectoryName( item.FileName );

                if(!String.IsNullOrWhiteSpace( path )) {
                    System.Diagnostics.Process.Start( path );
                }
            }
            catch (Exception ex) {
                mLog.LogException( "ProcessViewModel:Open file location", ex );
            }
        }

        private void UpdateItemStatus( ProcessItem item ) {
            var displayItem = GetDisplayedItem( item );

            if( displayItem != null ) {
                var handler = item.ProcessList.FirstOrDefault( i => i.ProcessState != ProcessState.Completed );

                displayItem.CurrentHandler = handler != null ? handler.Handler.HandlerName : "Completed";
                displayItem.CurrentState = handler?.ProcessState ?? ProcessState.Completed;

                if( displayItem.CurrentState == ProcessState.Error ) {
                    displayItem.SetProcessOutput( handler?.ProcessErrOut, handler?.OutputFileCreated == true );
                }

                lock( mProcessItems ) {
                    mHaveErroredProcess = mProcessItems.Any( i => i.HasError );
                }

                ContinueAll.RaiseCanExecuteChanged();
                AbortAll.RaiseCanExecuteChanged();
            }
        }

        private void DeleteItem( ProcessItem item ) {
            var displayItem = GetDisplayedItem( item );

            if( displayItem != null ) {
                lock( mProcessItems ) {
                    mProcessItems.Remove( displayItem );

                    mHaveErroredProcess = mProcessItems.Any( i => i.HasError );
                }

                ContinueAll.RaiseCanExecuteChanged();
                AbortAll.RaiseCanExecuteChanged();
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
